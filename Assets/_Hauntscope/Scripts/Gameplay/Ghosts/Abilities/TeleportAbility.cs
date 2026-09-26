using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    public sealed class TeleportAbility : IGhostAbility
    {
        private const float FullCircle = Mathf.PI * 2f;

        private readonly float _progressThreshold;
        private readonly float _minDistance;
        private readonly float _maxDistance;
        private readonly float _cooldown;
        private readonly float _staggerDuration;
        private float _cooldownRemaining;

        public TeleportAbility(float progressThreshold, float minDistance, float maxDistance, float cooldown, float staggerDuration = 0f)
        {
            _staggerDuration = staggerDuration;
            _progressThreshold = progressThreshold;
            _minDistance = minDistance;
            _maxDistance = maxDistance;
            _cooldown = cooldown;
        }

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
                return;
            }

            if (ghost.CaptureProgress <= _progressThreshold)
                return;

            var random = ghost.Context.Random;
            var angle = random.Range(0f, FullCircle);
            var distance = random.Range(_minDistance, _maxDistance);
            var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;

            ghost.TeleportTo(ghost.Position + offset);
            ghost.Stagger(_staggerDuration);
            _cooldownRemaining = _cooldown;
        }
    }
}
