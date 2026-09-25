using Hauntscope.Core.StateMachines;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.States
{
    public sealed class GhostEscapedState : IState
    {
        private readonly GhostContext _context;

        public GhostEscapedState(GhostContext context)
        {
            _context = context;
        }

        public float Progress { get; private set; }

        public bool IsFinished => Progress >= 1f;

        public void Enter()
        {
            Progress = 0f;
            _context.Mover.Stop();
        }

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
            Progress = Mathf.Clamp01(Progress + deltaTime / _context.Config.EscapeDuration);
        }
    }
}
