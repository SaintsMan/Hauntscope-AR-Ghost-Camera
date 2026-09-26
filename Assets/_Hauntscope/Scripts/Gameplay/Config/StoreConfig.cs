using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class StoreConfig
    {
        // The first laser is the standard issue every agent owns from the start.
        [SerializeField] private LaserData[] _lasers = Array.Empty<LaserData>();
        [SerializeField] private SpareBatteryData _spareBattery;
        [SerializeField] private BoosterData[] _boosters = Array.Empty<BoosterData>();
        [SerializeField, Range(0f, 1f)] private float _overflowRefund = 0.5f;

        public StoreConfig()
        {
        }

        public StoreConfig(LaserData[] lasers, SpareBatteryData spareBattery, BoosterData[] boosters)
        {
            _lasers = lasers;
            _spareBattery = spareBattery;
            _boosters = boosters;
        }

        public IReadOnlyList<LaserData> Lasers => _lasers;

        public LaserData DefaultLaser => _lasers.Length > 0 ? _lasers[0] : null;

        public SpareBatteryData SpareBattery => _spareBattery;

        public IReadOnlyList<BoosterData> Boosters => _boosters;

        // Gear rewarded beyond its stack limit is paid out as this share of its price.
        public float OverflowRefund => _overflowRefund;

        public LaserData FindLaser(string id)
        {
            foreach (var laser in _lasers)
            {
                if (laser.Id == id)
                    return laser;
            }

            return null;
        }
    }
}
