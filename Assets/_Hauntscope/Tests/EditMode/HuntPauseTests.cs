using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntPauseTests
    {
        private const float LostGrace = 0.5f;

        private FakeTrackingStatus _tracking;
        private FakeApplicationLifecycle _lifecycle;
        private HuntPause _pause;

        [SetUp]
        public void SetUp()
        {
            _tracking = new FakeTrackingStatus();
            _lifecycle = new FakeApplicationLifecycle();
            _pause = new HuntPause(_tracking, _lifecycle, new TrackingConfig(LostGrace));
            _pause.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _pause.Dispose();
        }

        [Test]
        public void Start_NoReasons_IsNotPaused()
        {
            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void PauseManually_Always_PausesAndRequestsMenu()
        {
            _pause.PauseManually();

            Assert.IsTrue(_pause.IsPaused);
            Assert.IsTrue(_pause.IsMenuRequested);
        }

        [Test]
        public void Resume_AfterManualPause_IsNotPaused()
        {
            _pause.PauseManually();

            _pause.Resume();

            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void ApplicationPaused_Always_PausesWithBackgroundReason()
        {
            _lifecycle.Pause();

            Assert.IsTrue(_pause.Has(PauseReason.Background));
            Assert.IsTrue(_pause.IsMenuRequested);
        }

        [Test]
        public void ApplicationResumed_AfterBackground_StaysPausedUntilPlayerResumes()
        {
            _lifecycle.Pause();

            _lifecycle.Resume();

            Assert.IsTrue(_pause.IsPaused);
        }

        [Test]
        public void Tick_NeverTracked_DoesNotReportTrackingLost()
        {
            _pause.Tick(LostGrace * 4f);

            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void Tick_LostShorterThanGrace_IsNotPaused()
        {
            _tracking.SetTracking(true);
            _tracking.SetTracking(false);

            _pause.Tick(LostGrace * 0.5f);

            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void Tick_LostLongerThanGrace_PausesWithoutMenu()
        {
            _tracking.SetTracking(true);
            _tracking.SetTracking(false);

            _pause.Tick(LostGrace);

            Assert.IsTrue(_pause.Has(PauseReason.TrackingLost));
            Assert.IsFalse(_pause.IsMenuRequested);
        }

        [Test]
        public void TrackingRegained_AfterLoss_Unpauses()
        {
            _tracking.SetTracking(true);
            _tracking.SetTracking(false);
            _pause.Tick(LostGrace);

            _tracking.SetTracking(true);

            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void TrackingRegained_WhilePausedManually_KeepsManualPause()
        {
            _tracking.SetTracking(true);
            _tracking.SetTracking(false);
            _pause.Tick(LostGrace);
            _pause.PauseManually();

            _tracking.SetTracking(true);

            Assert.IsTrue(_pause.Has(PauseReason.Manual));
            Assert.IsFalse(_pause.Has(PauseReason.TrackingLost));
        }

        [Test]
        public void Resume_WhileTrackingLost_StaysPaused()
        {
            _tracking.SetTracking(true);
            _tracking.SetTracking(false);
            _pause.Tick(LostGrace);
            _pause.PauseManually();

            _pause.Resume();

            Assert.IsTrue(_pause.Has(PauseReason.TrackingLost));
        }

        [Test]
        public void Tick_LostTwiceBriefly_GraceRestartsEachTime()
        {
            _tracking.SetTracking(true);
            _tracking.SetTracking(false);
            _pause.Tick(LostGrace * 0.6f);
            _tracking.SetTracking(true);
            _tracking.SetTracking(false);

            _pause.Tick(LostGrace * 0.6f);

            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void Dispose_AfterStart_IgnoresApplicationPause()
        {
            _pause.Dispose();

            _lifecycle.Pause();

            Assert.IsFalse(_pause.IsPaused);
        }
    }
}
