using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Store;

namespace Hauntscope.Gameplay.Iap
{
    // What a paid product may hand out. Products talk to this, not to the wallet, inventory and history one by one.
    public sealed class IapGrant
    {
        private readonly RewardGranter _granter;
        private readonly PlayerInventory _inventory;
        private readonly InventoryRepository _inventoryRepository;
        private readonly PurchaseHistory _history;

        public IapGrant(RewardGranter granter, PlayerInventory inventory, InventoryRepository inventoryRepository, PurchaseHistory history)
        {
            _granter = granter;
            _inventory = inventory;
            _inventoryRepository = inventoryRepository;
            _history = history;
        }

        public void AddEctoplasm(int amount)
        {
            if (amount > 0)
                _granter.Grant(new RewardBundle(amount));
        }

        // Gear past its stack limit turns into ectoplasm, like any other reward.
        public void Add(RewardBundle bundle)
        {
            if (bundle.Ectoplasm > 0 || bundle.Items.Count > 0)
                _granter.Grant(bundle);
        }

        // A bought laser goes straight into the agent's hands, as one bought for ectoplasm does.
        public void UnlockLaser(LaserData laser)
        {
            if (laser == null || _inventory.OwnsLaser(laser.Id))
                return;

            _inventory.AddLaser(laser.Id);
            _inventory.EquipLaser(laser.Id);
            _inventoryRepository.Save(_inventory);
        }

        public void UnlockPremium()
        {
            _history.SetPremium();
        }
    }
}
