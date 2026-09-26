using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "ShriekAbility", menuName = "Hauntscope/Abilities/Shriek")]
    public sealed class ShriekAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Min(0f)] private float _triggerDistance = 2.3f;
        [SerializeField, Range(0f, 1f)] private float _revealThreshold = 0.6f;
        [SerializeField, Min(0f)] private float _jamDuration = 2f;
        [SerializeField, Min(0f)] private float _cooldown = 9f;
        [SerializeField, Min(0f)] private float _staggerDuration = 1.5f;

        public override IGhostAbility CreateAbility()
        {
            return new ShriekAbility(_triggerDistance, _revealThreshold, _jamDuration, _cooldown, _staggerDuration);
        }
    }
}
