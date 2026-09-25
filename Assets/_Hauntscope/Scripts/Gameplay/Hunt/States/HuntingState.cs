using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Progress;
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
            ICameraPose camera)
        {
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

            var isFirstHunt = _progress.IsFirstSession;
            var data = _selector.Select(isFirstHunt);
            _progress.RegisterSession();
            _session.Begin(_factory.Create(data), data, isFirstHunt);
        }

        public void Exit()
        {
            _toolbelt.DeactivateAll();
            _radar.Reset();
        }

        public void Tick(float deltaTime)
        {
            var ghost = _session.Ghost.Value;

            _battery.Drain((_config.PassiveDrain + _toolbelt.TotalDrainPerSecond) * deltaTime);
            if (_battery.IsDepleted)
            {
                _toolbelt.DeactivateAll();
                ghost.Escape();
            }

            _toolbelt.Tick(deltaTime);
            ghost.Tick(deltaTime);
            _radar.Tick(deltaTime, ghost.Position, ghost.EmfRange);
            _session.AddTime(deltaTime);

            if (_scarePolicy.CanScare(_session, ghost, _camera))
            {
                ghost.Scare();
                _session.MarkScared();
            }

            if (_session.Result.Value != null)
                return;

            if (ghost.IsCaptureFinished)
                _session.Finish(HuntOutcome.Captured);
            else if (ghost.IsEscapeFinished)
                _session.Finish(HuntOutcome.Escaped);
        }
    }
}
