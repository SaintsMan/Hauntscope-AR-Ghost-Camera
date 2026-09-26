using Hauntscope.Gameplay.Engagement;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class RewardGranterTests
    {
        [Test]
        public void Grant_Ectoplasm_AddsAndSaves()
        {
            var store = new StoreFixture();
            var granter = Create(store);

            var paid = granter.Grant(new RewardBundle(15));

            Assert.AreEqual(15, paid);
            Assert.AreEqual(15, store.Progress.Ectoplasm.Value);
            Assert.AreEqual(15, store.ProgressRepository.Load().Ectoplasm.Value);
        }

        [Test]
        public void Grant_GearWithRoom_AddsItToTheInventory()
        {
            var store = new StoreFixture();

            Create(store).Grant(new RewardBundle(0, store.Salt, 2));

            Assert.AreEqual(2, store.Inventory.GetCount(StoreFixture.SaltId));
        }

        [Test]
        public void Grant_GearOverTheStackLimit_PaysTheRestAtHalfPrice()
        {
            var store = new StoreFixture();
            store.Inventory.AddGear(StoreFixture.BatteryId, StoreFixture.BatteryStack - 1);

            var paid = Create(store).Grant(new RewardBundle(10, store.Battery, 3));

            Assert.AreEqual(StoreFixture.BatteryStack, store.Inventory.GetCount(StoreFixture.BatteryId));
            Assert.AreEqual(10 + StoreFixture.BatteryPrice, paid);
            Assert.AreEqual(10 + StoreFixture.BatteryPrice, store.Progress.Ectoplasm.Value);
        }

        private static RewardGranter Create(StoreFixture store)
        {
            return new RewardGranter(store.Progress, store.ProgressRepository, store.Inventory, store.InventoryRepository, store.Config);
        }
    }
}
