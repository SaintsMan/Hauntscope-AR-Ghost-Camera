using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ThermalVisionTests
    {
        private const float Range = 4f;
        private const float FadeTime = 0.25f;

        private GhostFixture _fixture;
        private HuntSession _session;
        private FakeCameraPose _camera;
        private PlayerProgress _progress;
        private ThermalVision _thermal;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
            _fixture.Mover.Teleport(new Vector3(0f, 1.5f, 1f));
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            _camera = new FakeCameraPose { Position = new Vector3(0f, 1.5f, 0f) };
            _progress = new PlayerProgress();
            for (var i = 0; i < 4; i++)
                _progress.AddCapture("wisp", 10);
            _thermal = new ThermalVision(_session, _camera, TestConfigs.Thermal(range: Range, fadeTime: FadeTime), _progress);
        }

        [Test]
        public void Activate_TooFewCaptures_StaysOff()
        {
            var thermal = new ThermalVision(_session, _camera, TestConfigs.Thermal(unlockCaptures: 4), new PlayerProgress());

            thermal.Activate();

            Assert.IsFalse(thermal.IsActive.Value);
        }

        [Test]
        public void Tick_OnAndGhostInRange_ShowsItsColdShape()
        {
            _thermal.Activate();

            _thermal.Tick(FadeTime);

            Assert.AreEqual(1f, _fixture.Ghost.ThermalSight, 1e-4f);
        }

        [Test]
        public void Tick_OnAndGhostInRange_DoesNotRevealIt()
        {
            _thermal.Activate();

            _thermal.Tick(FadeTime);

            Assert.AreEqual(0f, _fixture.Ghost.VisibleReveal);
        }

        [Test]
        public void Tick_GhostBetweenFlickers_StillShows()
        {
            _fixture.Ghost.SetVisible(false);
            _thermal.Activate();

            _thermal.Tick(FadeTime);

            Assert.AreEqual(1f, _fixture.Ghost.ThermalSight, 1e-4f);
        }

        [Test]
        public void Tick_UndevelopedNegative_LeavesAColdPrint()
        {
            var negative = new GhostFixture(photoOnly: true);
            negative.Ghost.Start();
            negative.Mover.Teleport(new Vector3(0f, 1.5f, 1f));
            var session = new HuntSession();
            session.Begin(negative.Ghost, null);
            var thermal = new ThermalVision(session, _camera, TestConfigs.Thermal(fadeTime: FadeTime), _progress);
            thermal.Activate();

            thermal.Tick(FadeTime);

            Assert.AreEqual(1f, negative.Ghost.ThermalSight, 1e-4f);
        }

        [Test]
        public void Tick_GhostOutOfRange_ShowsNothing()
        {
            var thermal = new ThermalVision(_session, _camera, TestConfigs.Thermal(range: 0.5f, fadeTime: FadeTime), _progress);
            thermal.Activate();

            thermal.Tick(FadeTime);

            Assert.AreEqual(0f, _fixture.Ghost.ThermalSight);
        }

        [Test]
        public void Tick_GhostCaptured_ShowsNothing()
        {
            _fixture.Ghost.Capture();
            _fixture.Ghost.Tick(0.01f);
            _thermal.Activate();

            _thermal.Tick(FadeTime);

            Assert.AreEqual(0f, _fixture.Ghost.ThermalSight);
        }

        [Test]
        public void Tick_SwitchedOff_ShapeFades()
        {
            _thermal.Activate();
            _thermal.Tick(FadeTime);

            _thermal.Deactivate();
            _thermal.Tick(FadeTime * 0.5f);

            Assert.AreEqual(0.5f, _fixture.Ghost.ThermalSight, 1e-4f);
        }

        [Test]
        public void Ghost_ThermalSight_ReachesTheView()
        {
            _thermal.Activate();
            _thermal.Tick(FadeTime);

            _fixture.Ghost.Tick(0.01f);

            Assert.AreEqual(1f, _fixture.View.Thermal, 1e-4f);
        }
    }
}
