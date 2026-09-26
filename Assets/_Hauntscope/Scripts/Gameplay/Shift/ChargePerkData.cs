using UnityEngine;

namespace Hauntscope.Gameplay.Shift
{
    [CreateAssetMenu(fileName = "ChargePerk", menuName = "Hauntscope/Shift Perks/Charge")]
    public sealed class ChargePerkData : ShiftPerkData
    {
        [SerializeField, Range(0f, 1f)] private float _charge = 0.4f;

        public override IShiftPerk CreatePerk()
        {
            return new ChargePerk(_charge);
        }
    }
}
