using System;
using Hauntscope.Core.StateMachines;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.States
{
    public sealed class GhostFleeState : IState
    {
        private readonly GhostContext _context;
        private readonly Func<bool> _isBeamed;
        private readonly Func<float> _leash;
        private float _calmTime;
        private float _timeUntilRetarget;
        private bool _wasLeashed;

        public GhostFleeState(GhostContext context, Func<bool> isBeamed)
            : this(context, isBeamed, () => 0f)
        {
        }

        public GhostFleeState(GhostContext context, Func<bool> isBeamed, Func<float> leash)
        {
            _context = context;
            _isBeamed = isBeamed;
            _leash = leash;
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
            var beamed = _isBeamed();
            _calmTime = beamed ? 0f : _calmTime + deltaTime;

            // The beam catching it again must pull the target in at once, not after the next retarget.
            var leashed = beamed && _leash() > 0f;
            _timeUntilRetarget -= deltaTime;
            if (_timeUntilRetarget <= 0f || (leashed && !_wasLeashed))
                RetargetAwayFromCamera();
            _wasLeashed = leashed;

            _context.Mover.Tick(deltaTime, _context.Motion.FleeSpeed);
        }

        private void RetargetAwayFromCamera()
        {
            var mover = _context.Mover;
            var camera = _context.Camera.Position;
            var away = mover.Position - camera;
            away.y = 0f;
            if (away.sqrMagnitude <= 0f)
                away = -_context.Camera.Forward;
            away.y = 0f;

            var target = mover.Position + away.normalized * _context.Config.FleeDistance;
            if (_isBeamed())
                target = GhostLeash.Clamp(target, camera, _leash());

            target.y = mover.Position.y;
            mover.SetTarget(target);
            _timeUntilRetarget = _context.Config.FleeRetargetInterval;
        }
    }
}
