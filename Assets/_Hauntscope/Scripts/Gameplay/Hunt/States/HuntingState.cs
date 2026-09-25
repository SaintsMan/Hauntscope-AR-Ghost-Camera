using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Ghosts;
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

        public HuntingState(
            HuntSession session,
            GhostSelector selector,
            GhostFactory factory,
            EmfRadar radar,
            Toolbelt toolbelt)
        {
            _session = session;
            _selector = selector;
            _factory = factory;
            _radar = radar;
            _toolbelt = toolbelt;
        }

        public void Enter()
        {
            if (_session.Ghost.Value == null)
                _session.SetGhost(_factory.Create(_selector.Select()));
        }

        public void Exit()
        {
            _toolbelt.DeactivateAll();
            _radar.Reset();
        }

        public void Tick(float deltaTime)
        {
            var ghost = _session.Ghost.Value;
            _toolbelt.Tick(deltaTime);
            ghost.Tick(deltaTime);
            _radar.Tick(deltaTime, ghost.Position, ghost.EmfRange);
        }
    }
}
