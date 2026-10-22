using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class CrashReportContextTests
    {
        private FakeCrashReporter _reporter;
        private GameSettings _settings;
        private PurchaseHistory _purchases;
        private FakeLocalization _localization;
        private CrashReportContext _context;

        [SetUp]
        public void SetUp()
        {
            _reporter = new FakeCrashReporter();
            _settings = new SettingsRepository(new FakeSaveService()).Load();
            _purchases = new PurchaseHistory();
            _localization = new FakeLocalization();
            _context = new CrashReportContext(_reporter, _settings, _purchases, _localization);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void Start_Always_SetsEveryKey()
        {
            _context.Start();

            Assert.AreEqual(CrashKeys.SplashScreen, _reporter.KeyOf(CrashKeys.Screen));
            Assert.AreEqual(CrashKeys.Unknown, _reporter.KeyOf(CrashKeys.ArSupported));
            Assert.AreEqual("no", _reporter.KeyOf(CrashKeys.Premium));
            Assert.AreEqual("en", _reporter.KeyOf(CrashKeys.GameLanguage));
            Assert.IsNotNull(_reporter.KeyOf(CrashKeys.Mode));
        }

        [Test]
        public void Changes_AfterStart_FollowThePlayer()
        {
            _context.Start();

            _settings.SetEnvironment(HuntEnvironment.Virtual);
            _purchases.SetPremium();
            _localization.SetLanguage("uk");

            Assert.AreEqual("virtual", _reporter.KeyOf(CrashKeys.Mode));
            Assert.AreEqual("yes", _reporter.KeyOf(CrashKeys.Premium));
            Assert.AreEqual("uk", _reporter.KeyOf(CrashKeys.GameLanguage));
        }

        [Test]
        public void CheckAsync_NeedsInstall_PutsTheAnswerIntoTheContext()
        {
            var inner = new FakeArAvailability { Availability = ArAvailabilityResult.NeedsInstall };
            var availability = new ReportedArAvailability(inner, _reporter, new FakeAnalyticsService());

            var result = availability.CheckAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(ArAvailabilityResult.NeedsInstall, result);
            Assert.AreEqual("needs_install", _reporter.KeyOf(CrashKeys.ArSupported));
        }

        [Test]
        public void CheckAsync_Throws_ReportsAnArFailureAndRethrows()
        {
            var availability = new ReportedArAvailability(new ThrowingArAvailability(), _reporter, new FakeAnalyticsService());

            Assert.Throws<InvalidOperationException>(() => availability.CheckAsync(CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(1, _reporter.NonFatals.Count);
            StringAssert.StartsWith(CrashKeys.ArArea, _reporter.NonFatals[0]);
        }

        private sealed class ThrowingArAvailability : IArAvailability
        {
            public UniTask<ArAvailabilityResult> CheckAsync(CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("ARCore crashed");
            }

            public UniTask<bool> TryInstallAsync(CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("ARCore crashed");
            }
        }
    }
}
