using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class WitchingHourTests
    {
        private FakeClock _clock;
        private WitchingHour _witchingHour;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _witchingHour = new WitchingHour(_clock, new NightConfig(23, 4, 2f, 1.25f));
        }

        [TestCase(23, 0)]
        [TestCase(0, 30)]
        [TestCase(3, 59)]
        public void IsActive_DuringTheNight_IsTrue(int hour, int minute)
        {
            _clock.LocalNow = new DateTime(2026, 10, 31, hour, minute, 0);

            Assert.IsTrue(_witchingHour.IsActive);
        }

        [TestCase(4, 0)]
        [TestCase(12, 0)]
        [TestCase(22, 59)]
        public void IsActive_OutsideTheNight_IsFalse(int hour, int minute)
        {
            _clock.LocalNow = new DateTime(2026, 10, 31, hour, minute, 0);

            Assert.IsFalse(_witchingHour.IsActive);
        }

        [Test]
        public void LegendaryWeightMultiplier_ByDay_IsNeutral()
        {
            _clock.LocalNow = new DateTime(2026, 10, 31, 15, 0, 0);

            Assert.AreEqual(1f, _witchingHour.LegendaryWeightMultiplier);
        }

        [Test]
        public void LegendaryWeightMultiplier_AtNight_Doubles()
        {
            _clock.LocalNow = new DateTime(2026, 10, 31, 1, 0, 0);

            Assert.AreEqual(2f, _witchingHour.LegendaryWeightMultiplier);
        }
    }
}
