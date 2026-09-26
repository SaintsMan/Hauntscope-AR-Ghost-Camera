using System.Collections.Generic;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;

namespace Hauntscope.Gameplay.Shift
{
    // GDD 5.27: a night shift of hunts in a row. It keeps the round, the perks chosen at the breaks and what the shift
    // has earned; the hunt states ask it how to start each round and whether a result ends the shift. In a single
    // hunt it stays off and changes nothing.
    public sealed class NightShift
    {
        private readonly HuntLaunchOptions _options;
        private readonly ShiftConfig _config;
        private readonly ShiftDifficulty _difficulty;
        private readonly ShiftPerkPicker _picker;
        private readonly ShiftPerkTarget _target;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly RewardGranter _granter;
        private readonly StoreConfig _store;
        private readonly IRandom _random;
        private readonly List<IShiftPerk> _perks = new List<IShiftPerk>();
        private readonly List<ShiftPerkData> _chosen = new List<ShiftPerkData>();
        private readonly List<ShiftPerkData> _offer = new List<ShiftPerkData>();
        private readonly ObservableValue<ShiftPhase> _phase = new ObservableValue<ShiftPhase>(ShiftPhase.Off);

        public NightShift(
            HuntLaunchOptions options,
            ShiftConfig config,
            ShiftDifficulty difficulty,
            ShiftPerkPicker picker,
            ShiftPerkTarget target,
            PlayerProgress progress,
            PlayerProgressRepository repository,
            RewardGranter granter,
            StoreConfig store,
            IRandom random)
        {
            _options = options;
            _config = config;
            _difficulty = difficulty;
            _picker = picker;
            _target = target;
            _progress = progress;
            _repository = repository;
            _granter = granter;
            _store = store;
            _random = random;
        }

        public IReadOnlyObservableValue<ShiftPhase> Phase => _phase;

        public bool IsEnabled => _options.Mode == HuntMode.Shift;

        public bool IsRunning => _phase.Value == ShiftPhase.Hunting || _phase.Value == ShiftPhase.BetweenRounds
            || _phase.Value == ShiftPhase.Break;

        // Zero-based; shown to the player as round + 1 of Length.
        public int Round { get; private set; }

        public int Length => _config.Length;

        public bool IsFinalRound => Round >= _config.Length - 1;

        public float RewardMultiplier => IsRunning ? _difficulty.RewardMultiplier(Round) : 1f;

        // The pool for the round's ghost; null outside a shift, so the usual rarity weights apply.
        public RarityWeights Weights => IsRunning ? _difficulty.WeightsFor(Round) : null;

        public int ExtraFilm => _target.ExtraFilm;

        public float RoundRecharge => _config.RoundRecharge;

        // Everything the catches of this shift paid; the completion bonus comes on top.
        public int Earned { get; private set; }

        public bool IsCompleted { get; private set; }

        public RewardBundle CompletionBonus { get; private set; }

        public IReadOnlyList<ShiftPerkData> Offer => _offer;

        public IReadOnlyList<ShiftPerkData> Chosen => _chosen;

        // Before the round's ghost is picked. A hunt outside a running shift starts a new one. Returns whether boosters
        // are used up now: once per shift, at its start (a single hunt counts as its own start).
        public bool BeginRound()
        {
            if (!IsEnabled)
                return true;

            var starts = !IsRunning;
            if (starts)
            {
                Round = 0;
                Earned = 0;
                IsCompleted = false;
                CompletionBonus = null;
                _perks.Clear();
                _chosen.Clear();
            }

            _phase.Value = ShiftPhase.Hunting;
            // Recorded as the round begins, so a shift abandoned from the pause menu still counts how far it got.
            _progress.RecordShiftRound(Round + 1);
            _repository.Save(_progress);
            return starts;
        }

        // After the loadout has reset the modifiers: the round's difficulty, then every perk chosen so far.
        public void ApplyRound()
        {
            _target.BeginRound();
            if (!IsRunning)
                return;

            _target.Modifiers.Apply(_difficulty.ModifiersFor(Round));
            for (var i = 0; i < _perks.Count; i++)
                _perks[i].OnRoundStarted(_target);
        }

        // Once the hunt's result is banked. A catch before the last round leads on to a break; anything else ends it.
        public void RecordResult(HuntResult result)
        {
            if (!IsRunning || _phase.Value != ShiftPhase.Hunting)
                return;

            Earned += result.Reward;
            if (result.Outcome == HuntOutcome.Captured && !IsFinalRound)
            {
                _phase.Value = ShiftPhase.BetweenRounds;
                return;
            }

            End(result.Outcome == HuntOutcome.Captured);
        }

        public void OpenBreak()
        {
            if (_phase.Value != ShiftPhase.BetweenRounds)
                return;

            _picker.Pick(_config.Perks, _config.PerkOffer, _offer);
            if (_offer.Count > 0)
            {
                _phase.Value = ShiftPhase.Break;
                return;
            }

            // Nothing to offer: the break is skipped rather than left waiting for a choice that cannot be made.
            Round++;
            _phase.Value = ShiftPhase.Hunting;
        }

        public void Choose(int index)
        {
            if (_phase.Value != ShiftPhase.Break || index < 0 || index >= _offer.Count)
                return;

            var data = _offer[index];
            var perk = data.CreatePerk();
            _perks.Add(perk);
            _chosen.Add(data);
            perk.OnChosen(_target);
            Round++;
            _offer.Clear();
            _phase.Value = ShiftPhase.Hunting;
        }

        private void End(bool completed)
        {
            IsCompleted = completed;
            if (completed)
            {
                CompletionBonus = new RewardBundle(_config.CompleteBonus, RandomBooster(), 1);
                _progress.CompleteShift();
                _granter.Grant(CompletionBonus);
            }

            _phase.Value = ShiftPhase.Ended;
        }

        private GearData RandomBooster()
        {
            var boosters = _store.Boosters;
            return boosters.Count > 0 ? boosters[_random.Range(0, boosters.Count)] : null;
        }
    }
}
