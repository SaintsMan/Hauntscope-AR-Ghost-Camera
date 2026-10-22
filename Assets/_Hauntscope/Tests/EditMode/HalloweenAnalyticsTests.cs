using Hauntscope.Gameplay.Analytics;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HalloweenAnalyticsTests
    {
        private HalloweenFixture _event;
        private FakeAnalyticsService _analytics;
        private HalloweenAnalytics _reporter;

        [SetUp]
        public void SetUp()
        {
            _event = new HalloweenFixture();
            _analytics = new FakeAnalyticsService();
            _reporter = new HalloweenAnalytics(_analytics, _event.Collection);
            _reporter.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _reporter.Dispose();
        }

        [Test]
        public void CandyAdded_ReportsHowMany()
        {
            _event.Collection.Add(4);

            Assert.AreEqual("4", _analytics.ParameterOf(AnalyticsNames.CandyCollected, AnalyticsNames.Count));
        }

        [Test]
        public void Buy_ReportsTheSkin()
        {
            _event.Collection.Add(20);

            _event.Collection.Buy(_event.Beam);

            Assert.AreEqual(HalloweenFixture.BeamId, _analytics.ParameterOf(AnalyticsNames.SkinUnlocked, AnalyticsNames.ItemId));
        }

        [Test]
        public void EventOpened_ReportsTheScreen()
        {
            _reporter.EventOpened();

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.HalloweenEventOpen));
        }

        [Test]
        public void Dispose_StopsListening()
        {
            _reporter.Dispose();

            _event.Collection.Add(2);

            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.CandyCollected));
        }
    }
}
