using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Store
{
    // A purchase touches two saves (the wallet and the inventory), so both are written together right away.
    public sealed class Shop
    {
        private readonly PlayerProgress _progress;
        private readonly PlayerInventory _inventory;
        private readonly PlayerProgressRepository _progressRepository;
        private readonly InventoryRepository _inventoryRepository;

        public Shop(
            PlayerProgress progress,
            PlayerInventory inventory,
            PlayerProgressRepository progressRepository,
            InventoryRepository inventoryRepository)
        {
            _progress = progress;
            _inventory = inventory;
            _progressRepository = progressRepository;
            _inventoryRepository = inventoryRepository;
        }

        public bool CanAfford(int price)
        {
            return _progress.Ectoplasm.Value >= price;
        }

        // A new laser is equipped straight away: the player bought it to try it.
        public PurchaseResult BuyLaser(LaserData laser)
        {
            if (_inventory.OwnsLaser(laser.Id))
                return PurchaseResult.AlreadyOwned;

            if (!_progress.TrySpend(laser.Price))
                return PurchaseResult.NotEnoughEctoplasm;

            _inventory.AddLaser(laser.Id);
            _inventory.EquipLaser(laser.Id);
            SaveAll();
            return PurchaseResult.Purchased;
        }

        public bool EquipLaser(LaserData laser)
        {
            if (!_inventory.EquipLaser(laser.Id))
                return false;

            _inventoryRepository.Save(_inventory);
            return true;
        }

        // Bought gear is taken on the next hunt unless the player puts it back in the loadout row.
        public PurchaseResult BuyGear(GearData gear)
        {
            if (_inventory.GetCount(gear.Id) >= gear.MaxStack)
                return PurchaseResult.StackFull;

            if (!_progress.TrySpend(gear.Price))
                return PurchaseResult.NotEnoughEctoplasm;

            _inventory.AddGear(gear.Id, 1);
            _inventory.SetArmed(gear.Id, true);
            SaveAll();
            return PurchaseResult.Purchased;
        }

        public void SetArmed(GearData gear, bool armed)
        {
            _inventory.SetArmed(gear.Id, armed);
            _inventoryRepository.Save(_inventory);
        }

        private void SaveAll()
        {
            _progressRepository.Save(_progress);
            _inventoryRepository.Save(_inventory);
        }
    }
}
