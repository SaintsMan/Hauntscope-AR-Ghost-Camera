using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "NightmareAbility", menuName = "Hauntscope/Abilities/Nightmare")]
    public sealed class NightmareAbilityConfig : GhostAbilityConfig
    {
        // Any trace of the lens on her counts as light.
        [SerializeField, Range(0f, 1f)] private float _litThreshold = 0.05f;
        [SerializeField, Min(0.1f)] private float _lungeDistance = 1.3f;
        [SerializeField, Range(0f, 1f)] private float _bite = 0.2f;
        [SerializeField, Min(0f)] private float _bounceDistance = 3f;
        [SerializeField, Min(0.01f)] private float _bounceDuration = 0.35f;
        [SerializeField, Min(0f)] private float _spentStagger = 1.2f;
        [SerializeField, Min(0f)] private float _retreatDistance = 3.5f;

        public override IGhostAbility CreateAbility()
        {
            return new NightmareAbility(_litThreshold, _lungeDistance, _bite, _bounceDistance, _bounceDuration, _spentStagger,
                _retreatDistance);
        }
    }
}
