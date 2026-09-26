using Hauntscope.Gameplay.Tools;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class BatteryTests
    {
        private const float Max = 100f;

        private Battery _battery;

        [SetUp]
        public void SetUp()
        {
            _battery = new Battery(TestConfigs.Tools(batteryMax: Max, lowBatteryThreshold: 0.2f));
        }

        [Test]
        public void Constructor_Always_StartsFull()
        {
            Assert.AreEqual(Max, _battery.Charge.Value);
            Assert.AreEqual(1f, _battery.Normalized);
        }

        [Test]
        public void Drain_Amount_ReducesCharge()
        {
            _battery.Drain(30f);

            Assert.AreEqual(70f, _battery.Charge.Value);
        }

        [Test]
        public void Drain_MoreThanCharge_StopsAtZeroAndIsDepleted()
        {
            _battery.Drain(Max * 2f);

            Assert.AreEqual(0f, _battery.Charge.Value);
            Assert.IsTrue(_battery.IsDepleted);
        }

        [Test]
        public void Drain_BelowLowThreshold_IsLow()
        {
            _battery.Drain(81f);

            Assert.IsTrue(_battery.IsLow.Value);
        }

        [Test]
        public void Drain_AboveLowThreshold_IsNotLow()
        {
            _battery.Drain(79f);

            Assert.IsFalse(_battery.IsLow.Value);
        }

        [Test]
        public void Drain_CrossesLowThreshold_RaisesIsLowChanged()
        {
            var raised = false;
            _battery.IsLow.Changed += low => raised = low;

            _battery.Drain(90f);

            Assert.IsTrue(raised);
        }

        [Test]
        public void Refill_AfterDepletion_IsFullAndNotLow()
        {
            _battery.Drain(Max);

            _battery.Refill();

            Assert.AreEqual(Max, _battery.Charge.Value);
            Assert.IsFalse(_battery.IsLow.Value);
            Assert.IsFalse(_battery.IsDepleted);
        }

        [Test]
        public void DrainFraction_Quarter_TakesAQuarterOfAFullCharge()
        {
            _battery.DrainFraction(0.25f);

            Assert.AreEqual(Max * 0.75f, _battery.Charge.Value, 1e-4f);
        }
    }
}
