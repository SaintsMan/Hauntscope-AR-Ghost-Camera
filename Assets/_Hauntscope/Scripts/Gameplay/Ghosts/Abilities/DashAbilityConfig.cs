using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "DashAbility", menuName = "Hauntscope/Abilities/Dash")]
    public sealed class DashAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Min(0f)] private float _distance = 1.5f;
        [SerializeField, Min(0.01f)] private float _duration = 0.22f;
        [SerializeField, Min(0f)] private float _cooldown = 2f;

        public override IGhostAbility CreateAbility()
        {
            return new DashAbility(_distance, _duration, _cooldown);
        }
    }
}
