using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntLoadoutTests
    {
        [Test]
        public void Constructor_EquippedLaser_AppliesItsModifiers()
        {
            var store = new StoreFixture();
            store.Inventory.AddLaser(StoreFixture.FloodlightId);
            store.Inventory.EquipLaser(StoreFixture.FloodlightId);

            var loadout = store.CreateLoadout();

            Assert.AreSame(store.Floodlight, loadout.Laser);
            Assert.AreEqual(1.4f, store.Modifiers.ReticleRadius, 1e-4f);
        }

        [Test]
        public void Begin_ArmedBooster_ConsumesOneAndApplies()
        {
            var store = new StoreFixture();
            store.Inventory.AddGear(StoreFixture.SaltId, 2);
            store.Inventory.SetArmed(StoreFixture.SaltId, true);
            var loadout = store.CreateLoadout();

            loadout.Begin();

            Assert.AreEqual(1, store.Inventory.GetCount(StoreFixture.SaltId));
            Assert.AreEqual(0.65f, store.Modifiers.GhostSpeed, 1e-4f);
            CollectionAssert.AreEqual(new[] { store.Salt }, loadout.ActiveBoosters);
        }

        [Test]
        public void Begin_LaterShiftRound_ReappliesTheBoostersWithoutUsingMore()
        {
            var store = new StoreFixture();
            store.Inventory.AddGear(StoreFixture.SaltId, 2);
            store.Inventory.SetArmed(StoreFixture.SaltId, true);
            var loadout = store.CreateLoadout();
            loadout.Begin();

            loadout.Begin(false);

            Assert.AreEqual(1, store.Inventory.GetCount(StoreFixture.SaltId));
            Assert.AreEqual(0.65f, store.Modifiers.GhostSpeed, 1e-4f);
        }

        [Test]
        public void Begin_DisarmedBooster_IsKept()
        {
            var store = new StoreFixture();
            store.Inventory.AddGear(StoreFixture.AmpId, 1);
            var loadout = store.CreateLoadout();

            loadout.Begin();

            Assert.AreEqual(1, store.Inventory.GetCount(StoreFixture.AmpId));
            Assert.AreEqual(1f, store.Modifiers.EmfRange);
            Assert.IsEmpty(loadout.ActiveBoosters);
        }

        [Test]
        public void Begin_ArmedButOutOfStock_IsNotApplied()
        {
            var store = new StoreFixture();
            store.Inventory.SetArmed(StoreFixture.AmpId, true);
            var loadout = store.CreateLoadout();

            loadout.Begin();

            Assert.IsFalse(store.Modifiers.ShowsEmfDirection);
        }

        [Test]
        public void Begin_SecondHunt_DoesNotStackTheFirstHuntsBoosters()
        {
            var store = new StoreFixture();
            store.Inventory.AddGear(StoreFixture.SaltId, 1);
            store.Inventory.SetArmed(StoreFixture.SaltId, true);
            var loadout = store.CreateLoadout();
            loadout.Begin();

            loadout.Begin();

            Assert.AreEqual(1f, store.Modifiers.GhostSpeed);
            Assert.IsEmpty(loadout.ActiveBoosters);
        }

        [Test]
        public void Begin_BoosterUsed_SavesTheInventory()
        {
            var store = new StoreFixture();
            store.Inventory.AddGear(StoreFixture.AmpId, 1);
            store.Inventory.SetArmed(StoreFixture.AmpId, true);
            var loadout = store.CreateLoadout();

            loadout.Begin();

            Assert.AreEqual(0, store.InventoryRepository.Load().GetCount(StoreFixture.AmpId));
        }
    }
}
