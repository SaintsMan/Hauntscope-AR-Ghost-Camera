using Hauntscope.Gameplay.Store;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PlayerInventoryTests
    {
        private PlayerInventory _inventory;

        [SetUp]
        public void SetUp()
        {
            _inventory = new PlayerInventory(StoreFixture.StandardId);
        }

        [Test]
        public void Constructor_DefaultLaser_IsOwnedAndEquipped()
        {
            Assert.IsTrue(_inventory.OwnsLaser(StoreFixture.StandardId));
            Assert.AreEqual(StoreFixture.StandardId, _inventory.EquippedLaserId.Value);
        }

        [Test]
        public void EquipLaser_NotOwned_ReturnsFalse()
        {
            var equipped = _inventory.EquipLaser(StoreFixture.FloodlightId);

            Assert.IsFalse(equipped);
            Assert.AreEqual(StoreFixture.StandardId, _inventory.EquippedLaserId.Value);
        }

        [Test]
        public void TryConsume_LastItem_RemovesTheStack()
        {
            _inventory.AddGear(StoreFixture.AmpId, 1);

            var consumed = _inventory.TryConsume(StoreFixture.AmpId);

            Assert.IsTrue(consumed);
            Assert.AreEqual(0, _inventory.GetCount(StoreFixture.AmpId));
            Assert.IsFalse(_inventory.Gear.ContainsKey(StoreFixture.AmpId));
        }

        [Test]
        public void TryConsume_Empty_ReturnsFalse()
        {
            var consumed = _inventory.TryConsume(StoreFixture.AmpId);

            Assert.IsFalse(consumed);
        }

        [Test]
        public void AddGear_Amount_RaisesChanged()
        {
            var changed = 0;
            _inventory.Changed += () => changed++;

            _inventory.AddGear(StoreFixture.SaltId, 2);

            Assert.AreEqual(2, _inventory.GetCount(StoreFixture.SaltId));
            Assert.AreEqual(1, changed);
        }

        [Test]
        public void SetArmed_SameValue_DoesNotRaiseChanged()
        {
            _inventory.SetArmed(StoreFixture.AmpId, true);
            var changed = 0;
            _inventory.Changed += () => changed++;

            _inventory.SetArmed(StoreFixture.AmpId, true);

            Assert.AreEqual(0, changed);
        }
    }
}
