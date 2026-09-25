using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    public abstract class GhostAbilityConfig : ScriptableObject
    {
        public abstract IGhostAbility CreateAbility();
    }
}
