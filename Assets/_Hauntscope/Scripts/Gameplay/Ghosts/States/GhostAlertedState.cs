using System;
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
            _fastWander.TargetPicked += OnTargetPicked;
        }

        // The ghost freezes to stare at the camera: the ghost turns this moment into a stagger window.
        public event Action<float> PauseStarted;

        public event Action Retargeted;

        public void Enter()
        {
            _context.Mover.Stop();
            _context.Mover.FaceTowards(_context.Camera.Position);
            _pauseRemaining = _context.Config.AlertedPauseDuration;
            PauseStarted?.Invoke(_pauseRemaining);
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

        private void OnTargetPicked()
        {
            Retargeted?.Invoke();
        }
    }
}
