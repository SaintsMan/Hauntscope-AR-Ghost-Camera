using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "UndevelopedAbility", menuName = "Hauntscope/Abilities/Undeveloped")]
    public sealed class UndevelopedAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Min(0.5f)] private float _developDuration = 5f;

        public override IGhostAbility CreateAbility()
        {
            return new UndevelopedAbility(_developDuration);
        }
    }
}
