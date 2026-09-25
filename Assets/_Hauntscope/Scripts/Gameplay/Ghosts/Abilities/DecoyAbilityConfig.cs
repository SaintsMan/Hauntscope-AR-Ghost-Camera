using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "DecoyAbility", menuName = "Hauntscope/Abilities/Decoy")]
    public sealed class DecoyAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Min(0f)] private float _minDistance = 2.5f;
        [SerializeField, Min(0.1f)] private float _interval = 5f;

        public override IGhostAbility CreateAbility()
        {
            return new DecoyAbility(_minDistance, _interval);
        }
    }
}
