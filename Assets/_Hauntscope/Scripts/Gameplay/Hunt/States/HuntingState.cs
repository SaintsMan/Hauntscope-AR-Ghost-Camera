using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Hunt.States
{
    public sealed class HuntingState : IState
    {
        private readonly GhostSelector _selector;
        private readonly GhostFactory _factory;
        private readonly EmfRadar _radar;
        private Ghost _ghost;

        public HuntingState(GhostSelector selector, GhostFactory factory, EmfRadar radar)
        {
            _selector = selector;
            _factory = factory;
            _radar = radar;
        }

        public void Enter()
        {
            _ghost ??= _factory.Create(_selector.Select());
        }

        public void Exit()
        {
            _radar.Reset();
        }

        public void Tick(float deltaTime)
        {
            _ghost.Tick(deltaTime);
            _radar.Tick(deltaTime, _ghost.Position, _ghost.EmfRange);
        }
    }
}
