using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    // When and how many of one pickup appear in a hunt.
    [Serializable]
    public sealed class PickupSpawn
    {
        [SerializeField] private PickupData _pickup;
        [SerializeField] private PickupTrigger _trigger = PickupTrigger.HuntStart;
        [SerializeField, Range(0f, 1f)] private float _chance = 1f;
        [SerializeField, Min(0)] private int _countMin = 1;
        [SerializeField, Min(0)] private int _countMax = 1;
        [SerializeField, Range(0f, 1f)] private float _batteryBelow = 0.5f;

        public PickupSpawn()
        {
        }

        public PickupSpawn(PickupData pickup, PickupTrigger trigger, float chance, int countMin, int countMax, float batteryBelow = 0f)
        {
            _pickup = pickup;
            _trigger = trigger;
            _chance = chance;
            _countMin = countMin;
            _countMax = countMax;
            _batteryBelow = batteryBelow;
        }

        public PickupData Pickup => _pickup;

        public PickupTrigger Trigger => _trigger;

        public float Chance => _chance;

        public int CountMin => _countMin;

        public int CountMax => _countMax;

        // LowBattery only: fires once per hunt, the first time the charge drops below this fraction.
        public float BatteryBelow => _batteryBelow;
    }
}
