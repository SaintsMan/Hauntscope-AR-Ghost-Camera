using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SpareBatteriesTests
    {
        private StoreFixture _store;
        private Battery _battery;
        private SpareBatteries _spares;

        [SetUp]
        public void SetUp()
        {
            _store = new StoreFixture();
            _battery = new Battery(TestConfigs.Tools(batteryMax: 100f));
            _spares = new SpareBatteries(_store.Inventory, _store.InventoryRepository, _store.Config, _battery);
        }

        [Test]
        public void TryUse_HasSpareAndDrained_RechargesAndConsumes()
        {
            _store.Inventory.AddGear(StoreFixture.BatteryId, 2);
            _battery.Drain(70f);

            var used = _spares.TryUse();

            Assert.IsTrue(used);
            Assert.AreEqual(30f + StoreFixture.BatteryCharge * 100f, _battery.Charge.Value, 1e-3f);
            Assert.AreEqual(1, _spares.Count);
        }

        [Test]
        public void TryUse_FullBattery_KeepsTheSpare()
        {
            _store.Inventory.AddGear(StoreFixture.BatteryId, 1);

            var used = _spares.TryUse();

            Assert.IsFalse(used);
            Assert.AreEqual(1, _spares.Count);
        }

        [Test]
        public void TryUse_NoSpares_ReturnsFalse()
        {
            _battery.Drain(100f);

            var used = _spares.TryUse();

            Assert.IsFalse(used);
            Assert.IsTrue(_battery.IsDepleted);
        }

        [Test]
        public void TryUse_Used_SavesAndRaisesUsed()
        {
            _store.Inventory.AddGear(StoreFixture.BatteryId, 1);
            _battery.Drain(50f);
            var raised = false;
            _spares.Used += () => raised = true;

            _spares.TryUse();

            Assert.IsTrue(raised);
            Assert.AreEqual(0, _store.InventoryRepository.Load().GetCount(StoreFixture.BatteryId));
        }
    }
}
