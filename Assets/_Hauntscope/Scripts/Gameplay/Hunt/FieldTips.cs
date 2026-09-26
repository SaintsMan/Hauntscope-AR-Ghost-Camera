using System;
using System.Collections.Generic;
using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Shift;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Hunt
{
    // Field tips (GDD 5.31): new things are explained one at a time, the moment they first happen, and never again
    // (PlayerProgress.SeenTips). Silent during the tutorial hunt, which has its own hints. Ticked by HuntingState;
    // the night shift break, where the hunt does not tick, is checked when the phase changes.
    public sealed class FieldTips : IStartable, IDisposable
    {
        private readonly IReadOnlyList<IFieldTipRule> _rules;
        private readonly string[] _keys;
        private readonly FieldTipContext _context;
        private readonly HuntSession _session;
        private readonly NightShift _shift;
        private readonly TutorialFlow _tutorial;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly TipsConfig _config;
        private readonly ObservableValue<FieldTipId> _current = new ObservableValue<FieldTipId>(FieldTipId.None);

        private IFieldTipRule _shown;
        private float _shownTime;
        private Ghost _ghost;

        public FieldTips(IReadOnlyList<IFieldTipRule> rules, FieldTipContext context, HuntSession session, NightShift shift,
            TutorialFlow tutorial, PlayerProgress progress, PlayerProgressRepository repository, TipsConfig config)
        {
            _rules = rules;
            _context = context;
            _session = session;
            _shift = shift;
            _tutorial = tutorial;
            _progress = progress;
            _repository = repository;
            _config = config;
            _keys = new string[rules.Count];
            for (var i = 0; i < rules.Count; i++)
                _keys[i] = KeyOf(rules[i].Id);
        }

        public IReadOnlyObservableValue<FieldTipId> Current => _current;

        // The id a tip is remembered by in the save.
        public static string KeyOf(FieldTipId id)
        {
            return "tip." + id.ToString().ToLowerInvariant();
        }

        public void Start()
        {
            _session.Ghost.Changed += OnGhostChanged;
            _session.Result.Changed += OnResultChanged;
            _shift.Phase.Changed += OnPhaseChanged;
            OnGhostChanged(_session.Ghost.Value);
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= OnGhostChanged;
            _session.Result.Changed -= OnResultChanged;
            _shift.Phase.Changed -= OnPhaseChanged;
            Detach();
        }

        public void Tick(float deltaTime)
        {
            if (_tutorial.IsRunning)
                return;

            if (_ghost != null && !_ghost.IsStaggered)
                _context.IsFlushStagger = false;

            if (_shown != null)
            {
                _shownTime += deltaTime;
                if (_shown.IsResolved(_context) || _shownTime >= _config.Duration)
                    Hide();
                return;
            }

            Evaluate(deltaTime);
        }

        private void Evaluate(float deltaTime)
        {
            for (var i = 0; i < _rules.Count; i++)
            {
                if (_progress.HasSeenTip(_keys[i]) || !_rules[i].IsDue(_context, deltaTime))
                    continue;

                Show(_rules[i], _keys[i]);
                return;
            }
        }

        private void Show(IFieldTipRule rule, string key)
        {
            _shown = rule;
            _shownTime = 0f;
            _progress.MarkTipSeen(key);
            _repository.Save(_progress);
            _current.Value = rule.Id;
        }

        private void Hide()
        {
            _shown = null;
            _current.Value = FieldTipId.None;
        }

        private void OnGhostChanged(Ghost ghost)
        {
            Detach();
            Hide();
            _context.IsFlushStagger = false;
            for (var i = 0; i < _rules.Count; i++)
                _rules[i].Reset();
            _ghost = ghost;
            if (_ghost != null)
                _ghost.Flushed += OnFlushed;
        }

        private void Detach()
        {
            if (_ghost != null)
                _ghost.Flushed -= OnFlushed;
            _ghost = null;
        }

        private void OnFlushed()
        {
            _context.IsFlushStagger = true;
        }

        private void OnResultChanged(HuntResult result)
        {
            if (result != null)
                Hide();
        }

        private void OnPhaseChanged(ShiftPhase phase)
        {
            if (_tutorial.IsRunning)
                return;

            if (_shown != null && _shown.IsResolved(_context))
                Hide();
            if (_shown == null)
                Evaluate(0f);
        }
    }
}
