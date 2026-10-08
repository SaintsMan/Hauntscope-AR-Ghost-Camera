using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class RemindersTests
    {
        private IapFixture _iap;
        private FakeLocalNotifications _notifications;
        private FakeApplicationLifecycle _lifecycle;
        private NotificationOptIn _optIn;
        private Reminders _reminders;

        [SetUp]
        public void SetUp()
        {
            _iap = new IapFixture();
            _notifications = new FakeLocalNotifications();
            _lifecycle = new FakeApplicationLifecycle();
            var config = new NotificationConfig();
            var settingsRepository = new SettingsRepository(new FakeSaveService());
            _optIn = new NotificationOptIn(_notifications, settingsRepository.Load(), settingsRepository, _iap.Store.Progress, config);
            var ads = new FakeAdsService();
            var calendar = new LoginCalendar(new EngagementProgress(),
                new EngagementRepository(_iap.Store.Save, new ContractConfig(), _iap.Store.Config), new LoginConfig(),
                new RewardGranter(_iap.Store.Progress, _iap.Store.ProgressRepository, _iap.Store.Inventory, _iap.Store.InventoryRepository,
                    _iap.Store.Config), _iap.Clock, ads);
            _reminders = new Reminders(_notifications, _lifecycle, new FakeLocalization(), _iap.Clock, _optIn, calendar, _iap.Starter,
                new ReminderPlanner(config), config);
            _reminders.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _reminders.Dispose();
        }

        [Test]
        public void Start_RegistersBothChannels()
        {
            Assert.AreEqual(2, _notifications.Channels.Count);
        }

        [Test]
        public void Pause_NotificationsOn_LeavesLocalizedReminders()
        {
            _notifications.IsAllowed = true;

            _lifecycle.Pause();

            Assert.IsNotEmpty(_notifications.Scheduled);
            StringAssert.StartsWith("notify.", _notifications.Scheduled[0].Title);
            StringAssert.EndsWith(".title", _notifications.Scheduled[0].Title);
        }

        [Test]
        public void Pause_WithoutPermission_SchedulesNothing()
        {
            _lifecycle.Pause();

            Assert.IsEmpty(_notifications.Scheduled);
        }

        [Test]
        public void Resume_TakesTheRemindersBack()
        {
            _notifications.IsAllowed = true;
            _lifecycle.Pause();

            _lifecycle.Resume();

            Assert.IsEmpty(_notifications.Scheduled);
        }

        [Test]
        public void Pause_RookieKitRunning_WarnsBeforeItEnds()
        {
            _notifications.IsAllowed = true;
            _iap.PlayHunts(IapFixture.StarterAfterHunts);
            _iap.Starter.Refresh();
            _iap.Clock.Advance(System.TimeSpan.FromHours(IapFixture.StarterHours - 8));

            _lifecycle.Pause();

            Assert.IsTrue(_notifications.Scheduled.Exists(n => n.Kind == ReminderPlanner.Offer));
        }
    }
}
