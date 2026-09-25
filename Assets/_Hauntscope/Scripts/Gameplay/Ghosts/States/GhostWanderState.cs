using Hauntscope.Core.StateMachines;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.States
{
    public sealed class GhostWanderState : IState
    {
        private readonly GhostContext _context;
        private float _timeUntilNextTarget;

        public GhostWanderState(GhostContext context)
        {
            _context = context;
        }

        public void Enter()
        {
            PickNextTarget();
        }

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
            _timeUntilNextTarget -= deltaTime;
            if (_timeUntilNextTarget <= 0f)
                PickNextTarget();

            _context.Mover.Tick(deltaTime, _context.Motion.MoveSpeed);
        }

        private void PickNextTarget()
        {
            var bounds = _context.Planes.RoomBounds;
            var random = _context.Random;
            var target = new Vector3(
                random.Range(bounds.min.x, bounds.max.x),
                _context.Planes.FloorHeight + random.Range(_context.Motion.HoverHeightMin, _context.Motion.HoverHeightMax),
                random.Range(bounds.min.z, bounds.max.z));

            _context.Mover.SetTarget(target);
            _timeUntilNextTarget = random.Range(_context.Config.WanderIntervalMin, _context.Config.WanderIntervalMax);
        }
    }
}
