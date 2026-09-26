using Hauntscope.Gameplay.Ghosts.Abilities;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class BlinkAbilityTests
    {
        private const float VisibleDuration = 1.2f;
        private const float Period = 2f;

        private GhostFixture _fixture;
        private BlinkAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _ability = new BlinkAbility(VisibleDuration, Period);
        }

        [Test]
        public void Tick_InsideVisibleWindow_IsVisible()
        {
            _ability.Tick(_fixture.Ghost, VisibleDuration - 0.1f);

            Assert.IsTrue(_fixture.Ghost.IsVisible);
        }

        [Test]
        public void Tick_AfterVisibleWindow_IsHidden()
        {
            _ability.Tick(_fixture.Ghost, VisibleDuration + 0.1f);

            Assert.IsFalse(_fixture.Ghost.IsVisible);
        }

        [Test]
        public void Tick_NextPeriod_IsVisibleAgain()
        {
            _ability.Tick(_fixture.Ghost, VisibleDuration + 0.1f);

            _ability.Tick(_fixture.Ghost, Period - VisibleDuration);

            Assert.IsTrue(_fixture.Ghost.IsVisible);
        }

        [Test]
        public void Hidden_Revealed_HasNoVisibleReveal()
        {
            _fixture.Ghost.SetReveal(1f);

            _ability.Tick(_fixture.Ghost, VisibleDuration + 0.1f);

            Assert.AreEqual(0f, _fixture.Ghost.VisibleReveal);
            Assert.AreEqual(1f, _fixture.Ghost.Reveal);
        }

        [Test]
        public void Hidden_Beamed_CaptureDoesNotProgress()
        {
            _fixture.Ghost.Start();
            _fixture.Ghost.SetReveal(1f);
            var session = new HuntSession();
            session.Begin(_fixture.Ghost, null);
            var beam = new CaptureBeam(session, _fixture.Camera, TestConfigs.Tools(), new HuntModifiers());
            beam.Activate();
            _ability.Tick(_fixture.Ghost, VisibleDuration + 0.1f);

            beam.Tick(0.5f);

            Assert.AreEqual(0f, beam.Progress.Value);
        }

        [Test]
        public void Ghost_Captured_StaysVisibleDespiteBlink()
        {
            var ghost = new GhostFixture().Ghost;
            ghost.Start();
            ghost.Capture();

            ghost.SetVisible(false);

            Assert.IsTrue(ghost.IsVisible);
        }
    }
}
