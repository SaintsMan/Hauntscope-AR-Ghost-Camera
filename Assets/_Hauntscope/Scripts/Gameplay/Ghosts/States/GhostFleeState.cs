using System;
using Hauntscope.Core.StateMachines;

namespace Hauntscope.Gameplay.Ghosts.States
{
    public sealed class GhostFleeState : IState
    {
        private readonly GhostContext _context;
        private readonly Func<bool> _isBeamed;
        private float _calmTime;
        private float _timeUntilRetarget;

        public GhostFleeState(GhostContext context, Func<bool> isBeamed)
        {
            _context = context;
            _isBeamed = isBeamed;
        }

        public bool IsCalm => _calmTime >= _context.Config.FleeCalmDownTime;

        public void Enter()
        {
            _calmTime = 0f;
            RetargetAwayFromCamera();
        }

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
            _calmTime = _isBeamed() ? 0f : _calmTime + deltaTime;

            _timeUntilRetarget -= deltaTime;
            if (_timeUntilRetarget <= 0f)
                RetargetAwayFromCamera();

            _context.Mover.Tick(deltaTime, _context.Motion.FleeSpeed);
        }

        private void RetargetAwayFromCamera()
        {
            var mover = _context.Mover;
            var away = mover.Position - _context.Camera.Position;
            away.y = 0f;
            if (away.sqrMagnitude <= 0f)
                away = -_context.Camera.Forward;
            away.y = 0f;

            var target = mover.Position + away.normalized * _context.Config.FleeDistance;
            target.y = mover.Position.y;
            mover.SetTarget(target);
            _timeUntilRetarget = _context.Config.FleeRetargetInterval;
        }
    }
}
