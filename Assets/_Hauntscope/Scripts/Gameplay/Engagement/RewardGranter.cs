using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Engagement
{
    // Pays out a reward bundle and saves. Gear beyond its stack limit is not lost: it is paid out as ectoplasm at a
    // share of its price (GDD 5.30).
    public sealed class RewardGranter
    {
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _progressRepository;
        private readonly PlayerInventory _inventory;
        private readonly InventoryRepository _inventoryRepository;
        private readonly StoreConfig _store;

        public RewardGranter(PlayerProgress progress, PlayerProgressRepository progressRepository, PlayerInventory inventory,
            InventoryRepository inventoryRepository, StoreConfig store)
        {
            _progress = progress;
            _progressRepository = progressRepository;
            _inventory = inventory;
            _inventoryRepository = inventoryRepository;
            _store = store;
        }

        // Returns the ectoplasm actually paid, overflow refund included.
        public int Grant(RewardBundle bundle)
        {
            var ectoplasm = bundle.Ectoplasm;
            foreach (var item in bundle.Items)
            {
                var room = Mathf.Max(0, item.Gear.MaxStack - _inventory.GetCount(item.Gear.Id));
                var kept = Mathf.Min(room, item.Count);
                if (kept > 0)
                    _inventory.AddGear(item.Gear.Id, kept);
                ectoplasm += Mathf.RoundToInt((item.Count - kept) * item.Gear.Price * _store.OverflowRefund);
            }

            if (bundle.Items.Count > 0)
                _inventoryRepository.Save(_inventory);

            if (ectoplasm > 0)
                _progress.AddEctoplasm(ectoplasm);
            _progressRepository.Save(_progress);
            return ectoplasm;
        }
    }
}
