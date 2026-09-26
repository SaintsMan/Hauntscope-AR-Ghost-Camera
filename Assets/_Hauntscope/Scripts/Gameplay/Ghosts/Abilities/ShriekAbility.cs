using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The banshee punishes rushing in: revealed and approached too closely, she shrieks and drops out of the lens
    // for a moment, so the reveal and the beam's progress are lost unless the player keeps their distance.
    public sealed class ShriekAbility : IGhostAbility
    {
        private readonly float _triggerDistance;
        private readonly float _revealThreshold;
        private readonly float _jamDuration;
        private readonly float _cooldown;
        private readonly float _staggerDuration;
        private float _cooldownRemaining;
        private float _jamRemaining;

        public ShriekAbility(float triggerDistance, float revealThreshold, float jamDuration, float cooldown, float staggerDuration = 0f)
        {
            _staggerDuration = staggerDuration;
            _triggerDistance = triggerDistance;
            _revealThreshold = revealThreshold;
            _jamDuration = jamDuration;
            _cooldown = cooldown;
        }

        public bool IsJamming => _jamRemaining > 0f;

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (_jamRemaining > 0f)
            {
                _jamRemaining -= deltaTime;
                var isBack = _jamRemaining <= 0f;
                ghost.SetVisible(isBack);
                // The shriek spends her: she comes back out of the static exhausted.
                if (isBack)
                    ghost.Stagger(_staggerDuration);
                return;
            }

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
                return;
            }

            if (ghost.VisibleReveal < _revealThreshold)
                return;

            if (Vector3.Distance(ghost.Position, ghost.Context.Camera.Position) > _triggerDistance)
                return;

            ghost.Shriek();
            ghost.SetVisible(false);
            _jamRemaining = _jamDuration;
            _cooldownRemaining = _cooldown;
        }
    }
}
