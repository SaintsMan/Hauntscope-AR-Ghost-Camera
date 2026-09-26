using UnityEngine;

namespace Hauntscope.Gameplay.Store
{
    // Swapped in by hand during a hunt, or from the emergency charge card when the battery dies.
    [CreateAssetMenu(fileName = "SpareBattery", menuName = "Hauntscope/Spare Battery")]
    public sealed class SpareBatteryData : GearData
    {
        [SerializeField, Range(0f, 1f)] private float _charge = 0.5f;

        public float Charge => _charge;
    }
}
