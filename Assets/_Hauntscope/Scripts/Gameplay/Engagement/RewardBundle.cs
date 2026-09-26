using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Store;

namespace Hauntscope.Gameplay.Engagement
{
    // A reward handed out in one go: ectoplasm and any number of gear pieces.
    public sealed class RewardBundle
    {
        private readonly GearReward[] _items;

        public RewardBundle(int ectoplasm, GearData gear = null, int gearCount = 0)
            : this(ectoplasm, gear != null && gearCount > 0 ? new[] { new GearReward(gear, gearCount) } : Array.Empty<GearReward>())
        {
        }

        public RewardBundle(int ectoplasm, GearReward[] items)
        {
            Ectoplasm = ectoplasm;
            var kept = new List<GearReward>(items.Length);
            foreach (var item in items)
            {
                if (item != null && item.Gear != null && item.Count > 0)
                    kept.Add(item);
            }

            _items = kept.ToArray();
        }

        public int Ectoplasm { get; }

        public IReadOnlyList<GearReward> Items => _items;

        // The first piece of gear: most rewards carry at most one.
        public GearData Gear => _items.Length > 0 ? _items[0].Gear : null;

        public int GearCount => _items.Length > 0 ? _items[0].Count : 0;

        public RewardBundle WithEctoplasmTimes(int multiplier)
        {
            return new RewardBundle(Ectoplasm * multiplier, _items);
        }
    }
}
