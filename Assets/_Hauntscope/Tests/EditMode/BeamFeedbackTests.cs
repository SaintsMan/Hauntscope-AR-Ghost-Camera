using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class BeamFeedbackTests
    {
        private GhostFixture _fixture;
        private HuntSession _session;
        private CaptureBeam _beam;
        private HuntPause _pause;
        private FakeBeamView _view;
        private VfxConfig _config;
        private BeamFeedback _feedback;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 1f));
            _fixture.Ghost.Start();
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            _beam = new CaptureBeam(_session, _fixture.Camera, TestConfigs.Tools(), new HuntModifiers());
            _pause = new HuntPause(new FakeTrackingStatus(), new FakeApplicationLifecycle(), new TrackingConfig(0.5f));
            _view = new FakeBeamView();
            _config = new VfxConfig();
            _feedback = new BeamFeedback(_beam, _session, _pause, _fixture.Camera, _view, _config, new StoreFixture().CreateLoadout());
        }

        [Test]
        public void Tick_BeamInactive_KeepsBeamHidden()
        {
            _feedback.Tick();

            Assert.IsFalse(_view.IsVisible);
            Assert.AreEqual(0, _view.DrawCount);
        }

        [Test]
        public void Tick_BeamActiveWithoutLock_AimsStraightAheadAtRange()
        {
            _beam.Activate();

            _feedback.Tick();

            var camera = _fixture.Camera;
            Assert.IsTrue(_view.IsVisible);
            Assert.IsFalse(_view.IsLocked);
            Assert.AreEqual(camera.Position + camera.Forward * _config.BeamRange, _view.Target);
            Assert.AreEqual(_config.BeamIdleIntensity, _view.Intensity);
        }

        [Test]
        public void Tick_BeamActive_StartsBelowAndInFrontOfTheLens()
        {
            _beam.Activate();

            _feedback.Tick();

            var camera = _fixture.Camera;
            var expected = camera.Position + camera.Forward * _config.BeamOriginForward + Vector3.down * _config.BeamOriginDrop;
            Assert.AreEqual(expected, _view.Origin);
        }

        [Test]
        public void Tick_GhostLocked_BendsOntoGhost()
        {
            LockOnGhost();

            _feedback.Tick();

            Assert.IsTrue(_view.IsLocked);
            Assert.AreEqual(_fixture.Ghost.Position, _view.Target);
        }

        [Test]
        public void Tick_LockedAndCharging_BrightensWithProgress()
        {
            LockOnGhost();
            _feedback.Tick();
            var early = _view.Intensity;

            _beam.Tick(1f);
            _feedback.Tick();

            Assert.Greater(_view.Intensity, early);
        }

        [Test]
        public void Tick_Paused_HidesBeam()
        {
            _beam.Activate();
            _feedback.Tick();

            _pause.PauseManually();
            _feedback.Tick();

            Assert.IsFalse(_view.IsVisible);
        }

        [Test]
        public void Tick_GhostCaptured_HidesBeam()
        {
            _beam.Activate();
            _feedback.Tick();

            _fixture.Ghost.Capture();
            _fixture.Ghost.Tick(0.01f);
            _feedback.Tick();

            Assert.IsFalse(_view.IsVisible);
        }

        private void LockOnGhost()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();
            _beam.Tick(0.01f);
        }
    }
}
