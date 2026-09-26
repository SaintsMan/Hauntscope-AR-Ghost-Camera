using UnityEngine;

namespace Hauntscope.Gameplay.Store
{
    // Taken into a hunt from the loadout row and used up when the hunt starts.
    [CreateAssetMenu(fileName = "Booster", menuName = "Hauntscope/Booster")]
    public sealed class BoosterData : GearData
    {
        [SerializeField] private HuntModifierSet _modifiers = new HuntModifierSet();

        public HuntModifierSet Modifiers => _modifiers;
    }
}
