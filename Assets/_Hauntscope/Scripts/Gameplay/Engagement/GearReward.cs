using System;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Engagement
{
    // So many pieces of one gear, as a reward.
    [Serializable]
    public sealed class GearReward
    {
        [SerializeField] private GearData _gear;
        [SerializeField, Min(1)] private int _count = 1;

        public GearReward()
        {
        }

        public GearReward(GearData gear, int count)
        {
            _gear = gear;
            _count = count;
        }

        public GearData Gear => _gear;

        public int Count => _count;
    }
}
