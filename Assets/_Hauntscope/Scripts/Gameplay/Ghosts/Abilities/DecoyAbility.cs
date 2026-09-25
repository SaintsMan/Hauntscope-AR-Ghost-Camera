using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The mimic fakes its EMF signature somewhere else in the room until it is caught in the lens once.
    // Its whisper still comes from where it really is, so the player learns to trust their ears over the meter.
    public sealed class DecoyAbility : IGhostAbility
    {
        private const int Attempts = 16;

        private readonly float _minDistance;
        private readonly float _interval;
        private float _timeUntilMove;
        private bool _isExposed;

        public DecoyAbility(float minDistance, float interval)
        {
            _minDistance = minDistance;
            _interval = interval;
        }

        public bool IsExposed => _isExposed;

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (_isExposed)
                return;

            if (ghost.Reveal >= ghost.Context.Config.AlertRevealThreshold)
            {
                _isExposed = true;
                ghost.ClearEmfDecoy();
                return;
            }

            _timeUntilMove -= deltaTime;
            if (_timeUntilMove > 0f)
                return;

            ghost.SetEmfDecoy(PickDecoy(ghost));
            _timeUntilMove = _interval;
        }

        private Vector3 PickDecoy(Ghost ghost)
        {
            var context = ghost.Context;
            var bounds = context.Planes.RoomBounds;
            var origin = ghost.Position;
            var farthest = origin;
            var farthestDistance = -1f;

            for (var attempt = 0; attempt < Attempts; attempt++)
            {
                var candidate = new Vector3(
                    context.Random.Range(bounds.min.x, bounds.max.x),
                    origin.y,
                    context.Random.Range(bounds.min.z, bounds.max.z));
                var distance = Vector3.Distance(candidate, origin);
                if (distance >= _minDistance)
                    return candidate;

                if (distance > farthestDistance)
                {
                    farthest = candidate;
                    farthestDistance = distance;
                }
            }

            return farthest;
        }
    }
}
