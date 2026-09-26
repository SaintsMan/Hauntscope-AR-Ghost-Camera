using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "TeleportAbility", menuName = "Hauntscope/Abilities/Teleport")]
    public sealed class TeleportAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Range(0f, 1f)] private float _progressThreshold = 0.4f;
        [SerializeField, Min(0f)] private float _minDistance = 2f;
        [SerializeField, Min(0f)] private float _maxDistance = 3f;
        [SerializeField, Min(0f)] private float _cooldown = 5f;
        [SerializeField, Min(0f)] private float _staggerDuration = 1.2f;

        public override IGhostAbility CreateAbility()
        {
            return new TeleportAbility(_progressThreshold, _minDistance, _maxDistance, _cooldown, _staggerDuration);
        }
    }
}
