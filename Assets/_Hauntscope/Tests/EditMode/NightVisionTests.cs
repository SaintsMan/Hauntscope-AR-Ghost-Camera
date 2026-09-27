using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class NightVisionTests
    {
        private const float Glimpse = 0.2f;
        private const float Range = 5f;
        private const float FadeTime = 0.2f;

        private GhostFixture _fixture;
        private HuntSession _session;
        private FakeCameraPose _camera;
        private PlayerProgress _progress;
        private NightVision _night;

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
            _progress.AddCapture("wisp", 10);
            _progress.AddCapture("wisp", 10);
            _night = new NightVision(_session, _camera, TestConfigs.NightVision(glimpse: Glimpse, range: Range, farGlimpseScale: 1f,
                fadeTime: FadeTime), _progress);
        }

        [Test]
        public void Activate_NotIssuedYet_StaysOff()
        {
            var night = new NightVision(_session, _camera, TestConfigs.NightVision(unlockCaptures: 2), new PlayerProgress());

            night.Activate();

            Assert.IsFalse(night.IsActive.Value);
        }

        [Test]
        public void Tick_OnAndGhostInRange_ShowsItsFaintShape()
        {
            _night.Activate();

            _night.Tick(FadeTime);

            Assert.AreEqual(Glimpse, _fixture.Ghost.Glimpse, 1e-4f);
        }

        [Test]
        public void Tick_OnAndGhostInRange_DoesNotRevealIt()
        {
            _night.Activate();

            _night.Tick(FadeTime);

            Assert.AreEqual(0f, _fixture.Ghost.Reveal);
            Assert.AreEqual(0f, _fixture.Ghost.VisibleReveal);
        }

        [Test]
        public void Tick_GhostOutOfRange_ShowsNothing()
        {
            _fixture.Mover.Teleport(new Vector3(0f, 1.5f, 1.9f));
            var night = new NightVision(_session, _camera, TestConfigs.NightVision(range: 1f, fadeTime: FadeTime), _progress);
            night.Activate();

            night.Tick(FadeTime);

            Assert.AreEqual(0f, _fixture.Ghost.Glimpse);
        }

        [Test]
        public void Tick_GhostBetweenFlickers_ShowsNothing()
        {
            _fixture.Ghost.SetVisible(false);
            _night.Activate();

            _night.Tick(FadeTime);

            Assert.AreEqual(0f, _fixture.Ghost.Glimpse);
        }

        [Test]
        public void Tick_UndevelopedNegative_ShowsNothing()
        {
            var negative = new GhostFixture(photoOnly: true);
            negative.Ghost.Start();
            negative.Mover.Teleport(new Vector3(0f, 1.5f, 1f));
            var session = new HuntSession();
            session.Begin(negative.Ghost, null);
            var night = new NightVision(session, _camera, TestConfigs.NightVision(fadeTime: FadeTime), _progress);
            night.Activate();

            night.Tick(FadeTime);

            Assert.AreEqual(0f, negative.Ghost.Glimpse);
        }

        [Test]
        public void Tick_SwitchedOff_ShapeFadesAway()
        {
            _night.Activate();
            _night.Tick(FadeTime);

            _night.Deactivate();
            _night.Tick(FadeTime);

            Assert.AreEqual(0f, _fixture.Ghost.Glimpse, 1e-4f);
        }

        [Test]
        public void Tick_JustSwitchedOn_ShapeFadesIn()
        {
            _night.Activate();

            _night.Tick(FadeTime * 0.5f);

            Assert.AreEqual(Glimpse * 0.5f, _fixture.Ghost.Glimpse, 1e-4f);
        }
    }
}
