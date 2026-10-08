using System.Threading;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PushTopicsTests
    {
        private FakePushMessaging _push;
        private FakeLocalNotifications _notifications;
        private FakeLocalization _localization;
        private SettingsRepository _repository;
        private GameSettings _settings;
        private NotificationOptIn _optIn;
        private PushTopics _topics;

        [SetUp]
        public void SetUp()
        {
            _push = new FakePushMessaging();
            _notifications = new FakeLocalNotifications();
            _localization = new FakeLocalization();
            _repository = new SettingsRepository(new FakeSaveService());
            _settings = _repository.Load();
            var progress = new PlayerProgress();
            progress.AddCapture("wisp", 10);
            var config = new NotificationConfig();
            _optIn = new NotificationOptIn(_notifications, _settings, _repository, progress, config);
            _topics = new PushTopics(_push, _optIn, _settings, _repository, _localization, config);
        }

        [TearDown]
        public void TearDown()
        {
            _topics.Dispose();
        }

        [Test]
        public void Start_PlayerNotAskedYet_NeverTouchesThePushService()
        {
            _topics.Start();

            Assert.AreEqual(0, _push.Calls);
        }

        [Test]
        public void Accept_SubscribesToAllAndTheGameLanguage()
        {
            _topics.Start();

            _optIn.AcceptAsync(CancellationToken.None).GetAwaiter().GetResult();

            CollectionAssert.AreEquivalent(new[] { "all", "lang_en" }, _push.Topics);
            Assert.AreEqual("en", _repository.Load().PushLanguage);
        }

        [Test]
        public void LanguageChange_MovesToTheNewLanguageTopic()
        {
            _topics.Start();
            _optIn.AcceptAsync(CancellationToken.None).GetAwaiter().GetResult();

            _localization.SetLanguage("uk");

            CollectionAssert.AreEquivalent(new[] { "all", "lang_uk" }, _push.Topics);
        }

        [Test]
        public void TurningNotificationsOff_LeavesEveryTopic()
        {
            _topics.Start();
            _optIn.AcceptAsync(CancellationToken.None).GetAwaiter().GetResult();

            _optIn.SetEnabledAsync(false, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsEmpty(_push.Topics);
            Assert.AreEqual(string.Empty, _repository.Load().PushLanguage);
        }

        [Test]
        public void FailedSubscription_IsNotRemembered()
        {
            _push.Succeeds = false;
            _topics.Start();

            _optIn.AcceptAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(string.Empty, _settings.PushLanguage);
        }
    }
}
