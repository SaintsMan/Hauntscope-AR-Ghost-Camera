using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Pickups;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Research;
using Hauntscope.Gameplay.Shift;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Hunt.States
{
    public sealed class HuntingState : IState
    {
        private readonly HuntSession _session;
        private readonly GhostSelector _selector;
        private readonly GhostFactory _factory;
        private readonly EmfRadar _radar;
        private readonly Toolbelt _toolbelt;
        private readonly Battery _battery;
        private readonly ToolsConfig _config;
        private readonly PlayerProgress _progress;
        private readonly ScarePolicy _scarePolicy;
        private readonly ICameraPose _camera;
        private readonly HuntLoadout _loadout;
        private readonly HuntModifiers _modifiers;
        private readonly HuntLoot _loot;
        private readonly PickupField _pickups;
        private readonly EmergencyCharge _emergency;
        private readonly GhostResearch _research;
        private readonly SpiritCamera _spiritCamera;
        private readonly WitchingHour _witchingHour;
        private readonly NightShift _shift;

        public HuntingState(
            HuntSession session,
            GhostSelector selector,
            GhostFactory factory,
            EmfRadar radar,
            Toolbelt toolbelt,
            Battery battery,
            ToolsConfig config,
            PlayerProgress progress,
            ScarePolicy scarePolicy,
            ICameraPose camera,
            HuntLoadout loadout,
            HuntModifiers modifiers,
            HuntLoot loot,
            PickupField pickups,
            EmergencyCharge emergency,
            GhostResearch research,
            SpiritCamera spiritCamera,
            WitchingHour witchingHour,
            NightShift shift)
        {
            _shift = shift;
            _witchingHour = witchingHour;
            _spiritCamera = spiritCamera;
            _research = research;
            _loot = loot;
            _pickups = pickups;
            _emergency = emergency;
            _loadout = loadout;
            _modifiers = modifiers;
            _session = session;
            _selector = selector;
            _factory = factory;
            _radar = radar;
            _toolbelt = toolbelt;
            _battery = battery;
            _config = config;
            _progress = progress;
            _scarePolicy = scarePolicy;
            _camera = camera;
        }

        public void Enter()
        {
            if (_session.Ghost.Value != null)
                return;

            var consumeBoosters = _shift.BeginRound();
            var isFirstHunt = _progress.IsFirstSession;
            var data = _selector.Select(isFirstHunt, _shift.Weights);
            _progress.RegisterSession();
            _loot.Reset();
            _loadout.Begin(consumeBoosters);
            // Before the ghost is made: its speed is read from the modifiers when it spawns.
            _shift.ApplyRound();
            _emergency.ResetForHunt();
            _spiritCamera.BeginHunt(_shift.ExtraFilm);
            _session.Begin(_factory.Create(data), data, isFirstHunt, _witchingHour.IsActive);
            _pickups.Begin();
        }

        public void Exit()
        {
            _toolbelt.DeactivateAll();
            _radar.Reset();
            _pickups.Clear();
        }

        public void Tick(float deltaTime)
        {
            var ghost = _session.Ghost.Value;

            _battery.Drain((_config.PassiveDrain + _toolbelt.TotalDrainPerSecond) * deltaTime);
            if (_battery.IsDepleted && !ghost.IsCaptured && !ghost.IsEscaped)
            {
                // The emergency card pauses the hunt; the ghost only gets away once there is no way to recharge.
                if (_emergency.TryOffer())
                    return;

                _toolbelt.DeactivateAll();
                ghost.Escape();
            }

            _toolbelt.Tick(deltaTime);
            _spiritCamera.Tick(deltaTime);
            _pickups.Tick(deltaTime);
            ghost.Tick(deltaTime);
            _radar.Tick(deltaTime, ghost.EmfSource, ghost.EmfRange * _modifiers.EmfRange);
            _session.AddTime(deltaTime);

            if (!_session.IsSighted && ghost.VisibleReveal >= ghost.Context.Config.AlertRevealThreshold)
            {
                _session.MarkSighted();
                _progress.MarkSighted(_session.GhostData.Id);
            }

            if (_scarePolicy.CanScare(_session, ghost, _camera))
            {
                ghost.Scare();
                _session.MarkScared();
            }

            if (_session.Result.Value != null)
                return;

            var data = _session.GhostData;
            var photo = _spiritCamera.BestShot;
            var evidence = _spiritCamera.EvidenceShots;
            if (ghost.IsCaptureFinished)
                _session.Finish(HuntOutcome.Captured, _progress.GetCaptureCount(data.Id) == 0, _loot.Ectoplasm.Value,
                    _research.RewardMultiplier(data), _research.WillDeclassify(data, true, evidence), photo, _spiritCamera.Reward, evidence,
                    _session.IsWitchingHour ? _witchingHour.NightRewardMultiplier : 1f, _shift.RewardMultiplier);
            else if (ghost.IsEscapeFinished)
                _session.Finish(HuntOutcome.Escaped, false, _loot.Ectoplasm.Value, 1f,
                    _research.WillDeclassify(data, false, evidence), photo, _spiritCamera.Reward, evidence);
        }
    }
}
