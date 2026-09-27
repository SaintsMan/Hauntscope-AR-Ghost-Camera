using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Photo;

namespace Hauntscope.Gameplay.Hunt.States
{
    // The room is scanned and a prank photo is being set up: no ghost to hunt, no battery, only the posed one.
    public sealed class PrankState : IState
    {
        private readonly PrankPhotoMode _prank;

        public PrankState(PrankPhotoMode prank)
        {
            _prank = prank;
        }

        public void Enter()
        {
            _prank.Begin();
        }

        public void Exit()
        {
            _prank.End();
        }

        public void Tick(float deltaTime)
        {
            _prank.Tick(deltaTime);
        }
    }
}
