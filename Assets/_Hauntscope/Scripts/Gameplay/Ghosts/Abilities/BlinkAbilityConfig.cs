using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "BlinkAbility", menuName = "Hauntscope/Abilities/Blink")]
    public sealed class BlinkAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Min(0.01f)] private float _visibleDuration = 1.2f;
        [SerializeField, Min(0.01f)] private float _period = 2f;

        public override IGhostAbility CreateAbility()
        {
            return new BlinkAbility(_visibleDuration, _period);
        }
    }
}
