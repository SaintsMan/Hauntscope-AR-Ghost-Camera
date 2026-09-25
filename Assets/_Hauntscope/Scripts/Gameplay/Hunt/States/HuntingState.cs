using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Ghosts;

namespace Hauntscope.Gameplay.Hunt.States
{
    public sealed class HuntingState : IState
    {
        private readonly GhostSelector _selector;
        private readonly GhostFactory _factory;
        private Ghost _ghost;

        public HuntingState(GhostSelector selector, GhostFactory factory)
        {
            _selector = selector;
            _factory = factory;
        }

        public void Enter()
        {
            _ghost ??= _factory.Create(_selector.Select());
        }

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
            _ghost.Tick(deltaTime);
        }
    }
}
