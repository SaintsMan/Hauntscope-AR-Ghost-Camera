using Hauntscope.Gameplay.Analytics;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class AdsAnalyticsTests
    {
        private FakeAdsInitReport _report;
        private FakeRewardedReport _rewarded;
        private FakeAnalyticsService _analytics;
        private AdsAnalytics _ads;

        [SetUp]
        public void SetUp()
        {
            _report = new FakeAdsInitReport();
            _analytics = new FakeAnalyticsService();
            _rewarded = new FakeRewardedReport();
            _ads = new AdsAnalytics(_report, _rewarded, _analytics);
            _ads.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _ads.Dispose();
        }

        [Test]
        public void Reported_Error_LogsAdsInitWithResultAndAttempt()
        {
            _report.Report("error", 2);

            Assert.AreEqual("error", _analytics.ParameterOf(AnalyticsNames.AdsInit, AnalyticsNames.Result));
            Assert.AreEqual("2", _analytics.ParameterOf(AnalyticsNames.AdsInit, AnalyticsNames.Attempt));
        }

        [Test]
        public void Watched_Doubler_LogsRewardedWatchWithPlacement()
        {
            _rewarded.Report("doubler", "earned");

            Assert.AreEqual("doubler", _analytics.ParameterOf(AnalyticsNames.RewardedWatch, AnalyticsNames.Placement));
            Assert.AreEqual("earned", _analytics.ParameterOf(AnalyticsNames.RewardedWatch, AnalyticsNames.Result));
        }

        [Test]
        public void Reported_AfterDispose_LogsNothing()
        {
            _ads.Dispose();

            _report.Report("ok", 1);

            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.AdsInit));
        }
    }
}
