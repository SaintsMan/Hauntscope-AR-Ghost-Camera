using System.Threading;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class NotificationOptInTests
    {
        private FakeLocalNotifications _notifications;
        private FakeSaveService _save;
        private SettingsRepository _repository;
        private GameSettings _settings;
        private PlayerProgress _progress;
        private NotificationOptIn _optIn;

        [SetUp]
        public void SetUp()
        {
            _notifications = new FakeLocalNotifications();
            _save = new FakeSaveService();
            _repository = new SettingsRepository(_save);
            _settings = _repository.Load();
            _progress = new PlayerProgress();
            _optIn = new NotificationOptIn(_notifications, _settings, _repository, _progress, new NotificationConfig());
        }

        [Test]
        public void NeedsCard_BeforeTheFirstCatch_IsFalse()
        {
            Assert.IsFalse(_optIn.NeedsCard);
        }

        [Test]
        public void NeedsCard_AfterTheFirstCatchWithoutPermission_IsTrue()
        {
            _progress.AddCapture("wisp", 10);

            Assert.IsTrue(_optIn.NeedsCard);
        }

        [Test]
        public void SkipCardIfPointless_PermissionNotNeeded_AnswersYesSilently()
        {
            _progress.AddCapture("wisp", 10);
            _notifications.IsAllowed = true;

            _optIn.SkipCardIfPointless();

            Assert.IsTrue(_optIn.IsPrompted);
            Assert.IsTrue(_optIn.IsActive);
            Assert.AreEqual(0, _notifications.Requests);
        }

        [Test]
        public void AcceptAsync_AsksTheSystemAndTurnsOn()
        {
            _progress.AddCapture("wisp", 10);

            _optIn.AcceptAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _notifications.Requests);
            Assert.IsTrue(_optIn.IsActive);
            Assert.IsFalse(_optIn.NeedsCard);
        }

        [Test]
        public void Decline_NeverShowsTheSystemDialog()
        {
            _progress.AddCapture("wisp", 10);

            _optIn.Decline();

            Assert.AreEqual(0, _notifications.Requests);
            Assert.IsFalse(_optIn.IsActive);
            Assert.IsTrue(_repository.Load().NotificationsPrompted);
        }

        [Test]
        public void SetEnabledAsync_OnAfterDontAskAgain_OpensTheAppSettings()
        {
            _notifications.CanAsk = false;

            _optIn.SetEnabledAsync(true, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, _notifications.Requests);
            Assert.AreEqual(1, _notifications.SettingsOpened);
        }

        [Test]
        public void SetEnabledAsync_Off_StaysOffAcrossLoads()
        {
            _notifications.IsAllowed = true;

            _optIn.SetEnabledAsync(false, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_optIn.IsActive);
            Assert.IsFalse(_repository.Load().Notifications.Value);
        }
    }
}
