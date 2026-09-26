using Hauntscope.Gameplay.Store;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ShopTests
    {
        [Test]
        public void BuyLaser_EnoughEctoplasm_SpendsOwnsAndEquips()
        {
            var store = new StoreFixture(ectoplasm: 200);

            var result = store.Shop.BuyLaser(store.Floodlight);

            Assert.AreEqual(PurchaseResult.Purchased, result);
            Assert.AreEqual(200 - StoreFixture.FloodlightPrice, store.Progress.Ectoplasm.Value);
            Assert.IsTrue(store.Inventory.OwnsLaser(StoreFixture.FloodlightId));
            Assert.AreEqual(StoreFixture.FloodlightId, store.Inventory.EquippedLaserId.Value);
        }

        [Test]
        public void BuyLaser_NotEnoughEctoplasm_ChangesNothing()
        {
            var store = new StoreFixture(ectoplasm: 100);

            var result = store.Shop.BuyLaser(store.Floodlight);

            Assert.AreEqual(PurchaseResult.NotEnoughEctoplasm, result);
            Assert.AreEqual(100, store.Progress.Ectoplasm.Value);
            Assert.IsFalse(store.Inventory.OwnsLaser(StoreFixture.FloodlightId));
        }

        [Test]
        public void BuyLaser_AlreadyOwned_DoesNotChargeAgain()
        {
            var store = new StoreFixture(ectoplasm: 400);
            store.Shop.BuyLaser(store.Floodlight);

            var result = store.Shop.BuyLaser(store.Floodlight);

            Assert.AreEqual(PurchaseResult.AlreadyOwned, result);
            Assert.AreEqual(400 - StoreFixture.FloodlightPrice, store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void BuyLaser_Purchased_SavesWalletAndInventory()
        {
            var store = new StoreFixture(ectoplasm: 200);

            store.Shop.BuyLaser(store.Floodlight);

            Assert.AreEqual(200 - StoreFixture.FloodlightPrice, store.ProgressRepository.Load().Ectoplasm.Value);
            Assert.IsTrue(store.InventoryRepository.Load().OwnsLaser(StoreFixture.FloodlightId));
        }

        [Test]
        public void EquipLaser_NotOwned_KeepsCurrentLaser()
        {
            var store = new StoreFixture();

            var equipped = store.Shop.EquipLaser(store.Floodlight);

            Assert.IsFalse(equipped);
            Assert.AreEqual(StoreFixture.StandardId, store.Inventory.EquippedLaserId.Value);
        }

        [Test]
        public void EquipLaser_Owned_SwitchesBack()
        {
            var store = new StoreFixture(ectoplasm: 200);
            store.Shop.BuyLaser(store.Floodlight);

            var equipped = store.Shop.EquipLaser(store.Standard);

            Assert.IsTrue(equipped);
            Assert.AreEqual(StoreFixture.StandardId, store.Inventory.EquippedLaserId.Value);
        }

        [Test]
        public void BuyGear_EnoughEctoplasm_AddsOneAndArmsIt()
        {
            var store = new StoreFixture(ectoplasm: 100);

            var result = store.Shop.BuyGear(store.Amp);

            Assert.AreEqual(PurchaseResult.Purchased, result);
            Assert.AreEqual(1, store.Inventory.GetCount(StoreFixture.AmpId));
            Assert.IsTrue(store.Inventory.IsArmed(StoreFixture.AmpId));
            Assert.AreEqual(100 - StoreFixture.AmpPrice, store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void BuyGear_StackFull_RefusesWithoutCharging()
        {
            var store = new StoreFixture(ectoplasm: 1000);
            for (var i = 0; i < StoreFixture.BatteryStack; i++)
                store.Shop.BuyGear(store.Battery);
            var balance = store.Progress.Ectoplasm.Value;

            var result = store.Shop.BuyGear(store.Battery);

            Assert.AreEqual(PurchaseResult.StackFull, result);
            Assert.AreEqual(StoreFixture.BatteryStack, store.Inventory.GetCount(StoreFixture.BatteryId));
            Assert.AreEqual(balance, store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void BuyGear_NotEnoughEctoplasm_ChangesNothing()
        {
            var store = new StoreFixture(ectoplasm: 10);

            var result = store.Shop.BuyGear(store.Salt);

            Assert.AreEqual(PurchaseResult.NotEnoughEctoplasm, result);
            Assert.AreEqual(0, store.Inventory.GetCount(StoreFixture.SaltId));
        }

        [Test]
        public void SetArmed_False_IsSavedDisarmed()
        {
            var store = new StoreFixture(ectoplasm: 100);
            store.Shop.BuyGear(store.Amp);

            store.Shop.SetArmed(store.Amp, false);

            Assert.IsFalse(store.InventoryRepository.Load().IsArmed(StoreFixture.AmpId));
        }
    }
}
