using Hauntscope.Core.StateMachines;

namespace Hauntscope.Gameplay.Ghosts.States
{
    public sealed class GhostAlertedState : IState
    {
        private readonly GhostContext _context;
        private readonly GhostWanderState _fastWander;
        private float _pauseRemaining;

        public GhostAlertedState(GhostContext context)
        {
            _context = context;
            _fastWander = new GhostWanderState(context, context.Config.AlertedSpeedMultiplier);
        }

        public void Enter()
        {
            _context.Mover.Stop();
            _context.Mover.FaceTowards(_context.Camera.Position);
            _pauseRemaining = _context.Config.AlertedPauseDuration;
        }

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
            if (_pauseRemaining > 0f)
            {
                _pauseRemaining -= deltaTime;
                _context.Mover.FaceTowards(_context.Camera.Position);
                if (_pauseRemaining <= 0f)
                    _fastWander.Enter();
                return;
            }

            _fastWander.Tick(deltaTime);
        }
    }
}
