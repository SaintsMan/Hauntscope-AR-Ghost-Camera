using Hauntscope.Gameplay.Store;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class InventoryRepositoryTests
    {
        private const string Key = "player_inventory";

        [Test]
        public void Load_NoSave_OwnsOnlyTheDefaultLaser()
        {
            var store = new StoreFixture();

            var inventory = store.InventoryRepository.Load();

            Assert.AreEqual(1, inventory.Lasers.Count);
            Assert.AreEqual(StoreFixture.StandardId, inventory.EquippedLaserId.Value);
        }

        [Test]
        public void SaveThenLoad_Everything_RoundTrips()
        {
            var store = new StoreFixture();
            var inventory = store.Inventory;
            inventory.AddLaser(StoreFixture.FloodlightId);
            inventory.EquipLaser(StoreFixture.FloodlightId);
            inventory.AddGear(StoreFixture.BatteryId, 3);
            inventory.SetArmed(StoreFixture.SaltId, true);

            store.InventoryRepository.Save(inventory);
            var loaded = store.InventoryRepository.Load();

            Assert.IsTrue(loaded.OwnsLaser(StoreFixture.FloodlightId));
            Assert.AreEqual(StoreFixture.FloodlightId, loaded.EquippedLaserId.Value);
            Assert.AreEqual(3, loaded.GetCount(StoreFixture.BatteryId));
            Assert.IsTrue(loaded.IsArmed(StoreFixture.SaltId));
        }

        [Test]
        public void Load_EquippedLaserNoLongerSold_FallsBackToDefault()
        {
            var store = new StoreFixture();
            store.Save.SetRaw(Key, "{\"_version\":1,\"_lasers\":[\"retired\"],\"_equippedLaserId\":\"retired\"}");

            var inventory = store.InventoryRepository.Load();

            Assert.IsFalse(inventory.OwnsLaser("retired"));
            Assert.AreEqual(StoreFixture.StandardId, inventory.EquippedLaserId.Value);
        }

        [Test]
        public void Load_NewerVersion_StartsFresh()
        {
            var store = new StoreFixture();
            store.Save.SetRaw(Key, "{\"_version\":99,\"_lasers\":[\"floodlight\"],\"_equippedLaserId\":\"floodlight\"}");

            var inventory = store.InventoryRepository.Load();

            Assert.IsFalse(inventory.OwnsLaser(StoreFixture.FloodlightId));
        }
    }
}
