using System.Threading;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class NotificationAnalyticsTests
    {
        private FakeAnalyticsService _analytics;
        private FakePushMessaging _push;
        private FakeLocalNotifications _notifications;
        private FakeApplicationLifecycle _lifecycle;
        private NotificationOptIn _optIn;
        private PushTopics _topics;
        private NotificationAnalytics _tracker;

        [SetUp]
        public void SetUp()
        {
            _analytics = new FakeAnalyticsService();
            _push = new FakePushMessaging();
            _notifications = new FakeLocalNotifications();
            _lifecycle = new FakeApplicationLifecycle();
            var repository = new SettingsRepository(new FakeSaveService());
            var settings = repository.Load();
            var progress = new PlayerProgress();
            progress.AddCapture("wisp", 10);
            var config = new NotificationConfig();
            _optIn = new NotificationOptIn(_notifications, settings, repository, progress, config);
            _topics = new PushTopics(_push, _optIn, settings, repository, new FakeLocalization(), config);
            _tracker = new NotificationAnalytics(_analytics, _push, _topics, _notifications, _lifecycle);
        }

        [TearDown]
        public void TearDown()
        {
            _tracker.Dispose();
            _topics.Dispose();
        }

        [Test]
        public void PushReceived_TappedOpen_LogsPushOpenWithTheTopic()
        {
            _tracker.Start();

            _push.Raise("lang_uk", true);

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.PushOpen));
            Assert.AreEqual("lang_uk", _analytics.ParameterOf(AnalyticsNames.PushOpen, AnalyticsNames.Topic));
            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.PushReceived));
        }

        [Test]
        public void PushReceived_WhileTheGameRuns_LogsPushReceived()
        {
            _tracker.Start();

            _push.Raise("all", false);

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.PushReceived));
            Assert.AreEqual("all", _analytics.ParameterOf(AnalyticsNames.PushReceived, AnalyticsNames.Topic));
        }

        [Test]
        public void Start_LaunchedFromAReminder_LogsReminderOpenWithItsKind()
        {
            _notifications.OpenedKind = "tape";

            _tracker.Start();

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.ReminderOpen));
            Assert.AreEqual("tape", _analytics.ParameterOf(AnalyticsNames.ReminderOpen, AnalyticsNames.Kind));
        }

        [Test]
        public void Resume_BroughtBackByAReminder_LogsItOnce()
        {
            _tracker.Start();
            _notifications.OpenedKind = "witching";

            _lifecycle.Resume();
            _lifecycle.Resume();

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.ReminderOpen));
        }

        [Test]
        public void Accept_Subscribed_LogsPushSubscribedWithTheLanguage()
        {
            _tracker.Start();
            _topics.Start();

            _optIn.AcceptAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.PushSubscribed));
            Assert.AreEqual("en", _analytics.ParameterOf(AnalyticsNames.PushSubscribed, AnalyticsNames.Language));
        }

        [Test]
        public void Dispose_AfterStart_StopsListening()
        {
            _tracker.Start();

            _tracker.Dispose();
            _push.Raise("all", true);

            Assert.AreEqual(0, _analytics.Events.Count);
        }
    }
}
