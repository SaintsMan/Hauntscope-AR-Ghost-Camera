using System;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class FirstCaptureAnalyticsTests
    {
        private PlayerProgress _progress;
        private FakeInstallInfo _install;
        private FakeClock _clock;
        private FakeAnalyticsService _analytics;
        private FirstCaptureAnalytics _reporter;

        [SetUp]
        public void SetUp()
        {
            _progress = new PlayerProgress();
            _install = new FakeInstallInfo();
            _clock = new FakeClock();
            _analytics = new FakeAnalyticsService();
            _reporter = new FirstCaptureAnalytics(_progress, _install, _clock, _analytics);
        }

        [TearDown]
        public void TearDown()
        {
            _reporter.Dispose();
        }

        [Test]
        public void FirstCatch_InstallTimeKnown_ReportsSecondsSinceInstallAndTheHunt()
        {
            _install.InstalledAt = _clock.UtcNow - TimeSpan.FromMinutes(5);
            _reporter.Start();
            _progress.RegisterSession();
            _progress.RegisterSession();

            _progress.AddCapture("wisp", 10);

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.FirstCapture));
            Assert.AreEqual("300", _analytics.ParameterOf(AnalyticsNames.FirstCapture, AnalyticsNames.SinceInstallSeconds));
            Assert.AreEqual("2", _analytics.ParameterOf(AnalyticsNames.FirstCapture, AnalyticsNames.HuntNumber));
        }

        [Test]
        public void SecondCatch_ReportsNothingMore()
        {
            _reporter.Start();
            _progress.AddCapture("wisp", 10);

            _progress.AddCapture("wisp", 10);

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.FirstCapture));
        }

        [Test]
        public void Start_AgentCaughtBefore_NeverReports()
        {
            _progress.AddCapture("wisp", 10);
            _reporter.Start();

            _progress.AddCapture("wisp", 10);

            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.FirstCapture));
        }
    }
}
