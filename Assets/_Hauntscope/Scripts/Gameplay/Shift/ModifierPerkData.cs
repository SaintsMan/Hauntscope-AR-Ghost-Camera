using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Shift
{
    [CreateAssetMenu(fileName = "ModifierPerk", menuName = "Hauntscope/Shift Perks/Modifier")]
    public sealed class ModifierPerkData : ShiftPerkData
    {
        [SerializeField] private HuntModifierSet _modifiers = new HuntModifierSet();

        public override IShiftPerk CreatePerk()
        {
            return new ModifierPerk(_modifiers);
        }
    }
}
