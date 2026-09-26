using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Store
{
    public sealed class SpareBatteries
    {
        private readonly PlayerInventory _inventory;
        private readonly InventoryRepository _repository;
        private readonly StoreConfig _store;
        private readonly Battery _battery;

        public SpareBatteries(PlayerInventory inventory, InventoryRepository repository, StoreConfig store, Battery battery)
        {
            _inventory = inventory;
            _repository = repository;
            _store = store;
            _battery = battery;
        }

        public event Action Used;

        public int Count => _store.SpareBattery != null ? _inventory.GetCount(_store.SpareBattery.Id) : 0;

        // A full battery has no room for a spare, so the swap would waste it.
        public bool CanUse => Count > 0 && _battery.Normalized < 1f;

        public bool TryUse()
        {
            if (!CanUse || !_inventory.TryConsume(_store.SpareBattery.Id))
                return false;

            _battery.Recharge(_store.SpareBattery.Charge);
            _repository.Save(_inventory);
            Used?.Invoke();
            return true;
        }
    }
}
