using System;
using System.Collections.Generic;
using Hauntscope.Core.StateMachines;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.States
{
    // GDD 5.28: the ghost slips into a hiding spot and crouches there. The lens only finds it up close, a cold spot on
    // the floor gives it away, and once the lens pulls it out it is flushed: winded for a moment, then it bolts.
    public sealed class GhostHideState : IState
    {
        private readonly GhostContext _context;
        private readonly Func<bool> _isRevealed;
        private float _remaining;

        public GhostHideState(GhostContext context, Func<bool> isRevealed)
        {
            _context = context;
            _isRevealed = isRevealed;
        }

        public event Action<Vector3> Started;

        public event Action Flushed;

        public event Action Ended;

        // On the floor right under the ghost, where the frost shows.
        public Vector3 Spot { get; private set; }

        public bool IsFinished => _remaining <= 0f;

        public bool IsFlushed { get; private set; }

        public void Enter()
        {
            var config = _context.Hide;
            var random = _context.Random;
            var floor = _context.Planes.FloorHeight;
            var spot = PickSpot(_context.HideSpots.Spots, _context.Camera.Position);
            var height = floor - _context.Motion.BodyBottom + random.Range(config.FloorClearanceMin, config.FloorClearanceMax);

            IsFlushed = false;
            _remaining = random.Range(config.DurationMin, config.DurationMax);
            _context.Mover.SetTarget(new Vector3(spot.x, height, spot.z));
            var target = _context.Mover.Target;
            Spot = new Vector3(target.x, floor, target.z);
            Started?.Invoke(Spot);
        }

        public void Exit()
        {
            Ended?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            _remaining -= deltaTime;
            // It hurries into hiding, then settles there as the smoothing runs out of distance.
            _context.Mover.Tick(deltaTime, _context.Motion.FleeSpeed);
            if (IsFlushed || !_isRevealed())
                return;

            IsFlushed = true;
            Flushed?.Invoke();
        }

        // Any spot the player cannot already reach with the lens; if every spot is that close, the farthest one.
        private Vector3 PickSpot(IReadOnlyList<Vector3> spots, Vector3 player)
        {
            var minDistance = _context.Hide.MinDistance;
            var farEnough = 0;
            for (var i = 0; i < spots.Count; i++)
            {
                if (HorizontalDistance(spots[i], player) >= minDistance)
                    farEnough++;
            }

            if (farEnough == 0)
                return Farthest(spots, player);

            var pick = _context.Random.Range(0, farEnough);
            for (var i = 0; i < spots.Count; i++)
            {
                if (HorizontalDistance(spots[i], player) < minDistance)
                    continue;
                if (pick == 0)
                    return spots[i];
                pick--;
            }

            return spots[spots.Count - 1];
        }

        private static Vector3 Farthest(IReadOnlyList<Vector3> spots, Vector3 player)
        {
            var best = spots[0];
            for (var i = 1; i < spots.Count; i++)
            {
                if (HorizontalDistance(spots[i], player) > HorizontalDistance(best, player))
                    best = spots[i];
            }

            return best;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
