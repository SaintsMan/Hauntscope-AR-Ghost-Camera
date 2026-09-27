using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class UvFlashlightTests
    {
        private const float Lifetime = 20f;

        private GhostFixture _fixture;
        private HuntSession _session;
        private FakeCameraPose _camera;
        private PlayerProgress _progress;
        private UvFlashlight _uv;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
            _fixture.Mover.Teleport(new Vector3(0f, 1.2f, 1f));
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            _camera = new FakeCameraPose { Position = new Vector3(0f, 1.5f, -1f), Forward = new Vector3(0f, -0.5f, 1f).normalized };
            _progress = new PlayerProgress();
            for (var i = 0; i < 6; i++)
                _progress.AddCapture("wisp", 10);
            _uv = new UvFlashlight(_session, _camera, new FakePlaneProvider(), TestConfigs.Uv(range: 4f, coneAngle: 40f, coneSoftness: 5f,
                trailSpacing: 0.5f, trailLifetime: Lifetime, fadeStart: 0.5f), _progress);
        }

        [Test]
        public void Activate_TooFewCaptures_StaysOff()
        {
            var uv = new UvFlashlight(_session, _camera, new FakePlaneProvider(), TestConfigs.Uv(unlockCaptures: 6), new PlayerProgress());

            uv.Activate();

            Assert.IsFalse(uv.IsActive.Value);
        }

        [Test]
        public void Tick_BeamOff_StillRecordsThePath()
        {
            WalkGhost();

            Assert.IsFalse(_uv.Trail[0].IsEmpty);
        }

        [Test]
        public void Tick_BeamOff_ShowsNothing()
        {
            WalkGhost();

            Assert.AreEqual(0f, _uv.VisibilityOf(0));
        }

        [Test]
        public void Tick_BeamOnAndMarkInTheCone_ShowsIt()
        {
            WalkGhost();
            _uv.Activate();

            _uv.Tick(0.01f);

            Assert.Greater(_uv.VisibilityOf(0), 0.9f);
        }

        [Test]
        public void Tick_MarkBehindTheBeam_ShowsNothing()
        {
            WalkGhost();
            _uv.Activate();
            _camera.Forward = Vector3.back;

            _uv.Tick(0.01f);

            Assert.AreEqual(0f, _uv.VisibilityOf(0));
        }

        [Test]
        public void Tick_MarkPastItsLifetime_ShowsNothing()
        {
            WalkGhost();
            _uv.Activate();

            _uv.Tick(Lifetime);

            Assert.AreEqual(0f, _uv.VisibilityOf(0));
        }

        [Test]
        public void Tick_MarkAgeing_FadesPastFadeStart()
        {
            WalkGhost();
            _uv.Activate();

            _uv.Tick(Lifetime * 0.75f);

            Assert.AreEqual(0.5f, _uv.VisibilityOf(0), 0.05f);
        }

        [Test]
        public void Teleport_Always_SplashesWhereItLeftAndLanded()
        {
            _uv.Tick(0.01f);

            _fixture.Ghost.TeleportTo(new Vector3(1f, 1.2f, 1.5f));

            Assert.AreEqual(TrailMarkKind.Splash, _uv.Trail[0].Kind);
            Assert.AreEqual(TrailMarkKind.Splash, _uv.Trail[1].Kind);
        }

        [Test]
        public void Marks_Always_LieOnTheFloor()
        {
            WalkGhost();

            Assert.AreEqual(0f, _uv.Trail[0].Position.y, 0.05f);
        }

        [Test]
        public void Tick_NewGhost_StartsAFreshTrail()
        {
            WalkGhost();
            var next = new GhostFixture();
            next.Ghost.Start();

            _session.Begin(next.Ghost, null);
            _uv.Tick(0.01f);

            Assert.IsTrue(_uv.Trail[0].IsEmpty);
        }

        [Test]
        public void Lights_BeamOnAndPointInTheCone_IsTrue()
        {
            _uv.Activate();

            Assert.IsTrue(_uv.Lights(new Vector3(0f, 0.5f, 0.5f)));
        }

        [Test]
        public void Lights_BeamOff_IsFalse()
        {
            Assert.IsFalse(_uv.Lights(new Vector3(0f, 0.5f, 0.5f)));
        }

        private void WalkGhost()
        {
            _uv.Tick(0.01f);
            _fixture.Mover.Teleport(new Vector3(0f, 1.2f, 1.6f));
            _uv.Tick(0.01f);
        }
    }
}
