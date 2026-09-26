using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "HomebodyAbility", menuName = "Hauntscope/Abilities/Homebody")]
    public sealed class HomebodyAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Min(0.1f)] private float _knockIntervalMin = 1.6f;
        [SerializeField, Min(0.1f)] private float _knockIntervalMax = 2.4f;
        [SerializeField, Min(0f)] private float _firstKnock = 0.8f;

        public override IGhostAbility CreateAbility()
        {
            return new HomebodyAbility(_knockIntervalMin, _knockIntervalMax, _firstKnock);
        }
    }
}
