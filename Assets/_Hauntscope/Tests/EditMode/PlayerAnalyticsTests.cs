using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PlayerAnalyticsTests
    {
        private FakeAnalyticsService _analytics;
        private GameSettings _settings;
        private PurchaseHistory _purchases;
        private PlayerProgress _progress;
        private FakeLocalization _localization;
        private PlayerAnalytics _player;

        [SetUp]
        public void SetUp()
        {
            _analytics = new FakeAnalyticsService();
            _settings = new SettingsRepository(new FakeSaveService()).Load();
            _purchases = new PurchaseHistory();
            _progress = new PlayerProgress();
            _localization = new FakeLocalization();
            _player = new PlayerAnalytics(_analytics, _settings, _purchases, _progress, _localization);
        }

        [TearDown]
        public void TearDown()
        {
            _player.Dispose();
        }

        [Test]
        public void Start_Always_SetsEveryUserProperty()
        {
            _player.Start();

            Assert.AreEqual("no", _analytics.UserProperties[AnalyticsNames.PremiumProperty]);
            Assert.AreEqual("en", _analytics.UserProperties[AnalyticsNames.GameLanguageProperty]);
            Assert.IsTrue(_analytics.UserProperties.ContainsKey(AnalyticsNames.EnvironmentProperty));
            Assert.IsTrue(_analytics.UserProperties.ContainsKey(AnalyticsNames.NotificationsProperty));
            Assert.AreEqual(0, _analytics.Events.Count);
        }

        [Test]
        public void SetPremium_AfterStart_UpdatesProperty()
        {
            _player.Start();

            _purchases.SetPremium();

            Assert.AreEqual("yes", _analytics.UserProperties[AnalyticsNames.PremiumProperty]);
        }

        [Test]
        public void SetEnvironment_Virtual_UpdatesProperty()
        {
            _player.Start();

            _settings.SetEnvironment(HuntEnvironment.Virtual);

            Assert.AreEqual("virtual", _analytics.UserProperties[AnalyticsNames.EnvironmentProperty]);
        }

        [Test]
        public void SetLanguage_Ukrainian_UpdatesProperty()
        {
            _player.Start();

            _localization.SetLanguage("uk");

            Assert.AreEqual("uk", _analytics.UserProperties[AnalyticsNames.GameLanguageProperty]);
        }

        [Test]
        public void MarkTutorialCompleted_FirstTime_LogsOnce()
        {
            _player.Start();

            _progress.MarkTutorialCompleted();
            _progress.MarkReviewPrompted();

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.TutorialComplete));
        }

        [Test]
        public void MarkTutorialCompleted_AlreadyDoneBeforeStart_LogsNothing()
        {
            _progress.MarkTutorialCompleted();
            _player.Start();

            _progress.MarkTutorialCompleted();

            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.TutorialComplete));
        }

        [Test]
        public void SetNotifications_Changed_LogsAnswerAndProperty()
        {
            _player.Start();
            var enabled = !_settings.Notifications.Value;

            _settings.SetNotifications(enabled);

            Assert.AreEqual(AnalyticsNames.Of(enabled), _analytics.ParameterOf(AnalyticsNames.NotificationsSet, AnalyticsNames.Enabled));
            Assert.AreEqual(AnalyticsNames.Of(enabled), _analytics.UserProperties[AnalyticsNames.NotificationsProperty]);
        }

        [Test]
        public void Dispose_AfterStart_StopsListening()
        {
            _player.Start();
            _player.Dispose();

            _progress.MarkTutorialCompleted();

            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.TutorialComplete));
        }
    }
}
