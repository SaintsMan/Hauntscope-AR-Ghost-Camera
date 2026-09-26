using Hauntscope.Gameplay.Ghosts;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostTests
    {
        private GhostFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(1f, 1f, 1f));
        }

        [Test]
        public void Start_Always_PlacesViewAtGhost()
        {
            _fixture.Ghost.Start();

            Assert.AreEqual(_fixture.Mover.VisualPosition, _fixture.View.Position);
        }

        [Test]
        public void Start_Always_HidesGhost()
        {
            _fixture.Ghost.Start();

            Assert.AreEqual(0f, _fixture.View.Reveal);
        }

        [Test]
        public void Tick_AfterStart_MovesViewWithGhost()
        {
            _fixture.Ghost.Start();

            _fixture.Ghost.Tick(0.1f);

            Assert.AreEqual(_fixture.Mover.VisualPosition, _fixture.View.Position);
            Assert.AreEqual(2, _fixture.View.SetPoseCount);
        }

        [Test]
        public void SetReveal_OutOfRange_IsClamped()
        {
            _fixture.Ghost.SetReveal(1.7f);

            Assert.AreEqual(1f, _fixture.Ghost.Reveal);
        }

        [Test]
        public void Tick_RevealBelowAlertThreshold_KeepsWandering()
        {
            _fixture.Ghost.Start();
            _fixture.Ghost.SetReveal(GhostFixture.AlertThreshold - 0.1f);

            _fixture.Ghost.Tick(0.1f);

            Assert.IsFalse(_fixture.Ghost.IsAlerted);
        }

        [Test]
        public void Tick_RevealReachesAlertThreshold_BecomesAlerted()
        {
            _fixture.Ghost.Start();
            _fixture.Ghost.SetReveal(GhostFixture.AlertThreshold);

            _fixture.Ghost.Tick(0.1f);

            Assert.IsTrue(_fixture.Ghost.IsAlerted);
        }

        [Test]
        public void Tick_Revealed_PassesRevealToView()
        {
            _fixture.Ghost.Start();
            _fixture.Ghost.SetReveal(0.3f);

            _fixture.Ghost.Tick(0.1f);

            Assert.AreEqual(0.3f, _fixture.View.Reveal, 1e-5f);
        }

        [Test]
        public void Tick_BeamedWhileCharging_StrugglesWithCaptureProgress()
        {
            _fixture.Ghost.Start();
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.SetBeamed(true);
            _fixture.Ghost.SetCaptureProgress(0.6f);

            _fixture.Ghost.Tick(0.01f);

            Assert.AreEqual(0.6f, _fixture.View.Struggle, 1e-5f);
        }

        [Test]
        public void Tick_NotBeamed_DoesNotStruggle()
        {
            _fixture.Ghost.Start();
            _fixture.Ghost.SetCaptureProgress(0.6f);

            _fixture.Ghost.Tick(0.01f);

            Assert.AreEqual(0f, _fixture.View.Struggle);
        }

        [Test]
        public void Tick_Salted_MovesSlower()
        {
            var fixture = new GhostFixture();
            fixture.Ghost.SetSpeedModifiers(0.5f, 1f);
            fixture.Ghost.Start();

            fixture.Ghost.Tick(0.02f);

            Assert.AreEqual(0.5f, fixture.Mover.SpeedScale, 1e-5f);
        }

        [Test]
        public void Tick_BeamedWithTether_StacksBothSlowdowns()
        {
            var fixture = new GhostFixture();
            fixture.Ghost.SetSpeedModifiers(0.5f, 0.5f);
            fixture.Ghost.Start();
            fixture.Ghost.SetBeamed(true);

            fixture.Ghost.Tick(0.02f);

            Assert.AreEqual(0.25f, fixture.Mover.SpeedScale, 1e-5f);
        }
    
        [Test]
        public void Tick_Calm_TellsTheViewItIsCalm()
        {
            _fixture.Ghost.Start();

            _fixture.Ghost.Tick(0.1f);

            Assert.AreEqual(GhostMood.Calm, _fixture.View.Mood);
        }

        [Test]
        public void Tick_Noticed_TellsTheViewItIsAlert()
        {
            _fixture.Ghost.Start();
            _fixture.Ghost.SetReveal(1f);

            _fixture.Ghost.Tick(0.1f);

            Assert.AreEqual(GhostMood.Alert, _fixture.View.Mood);
        }

        [Test]
        public void Tick_UnderTheBeam_TellsTheViewItIsFleeing()
        {
            _fixture.Ghost.Start();
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Tick(0.1f);
            _fixture.Ghost.SetBeamed(true);

            _fixture.Ghost.Tick(0.1f);

            Assert.AreEqual(GhostMood.Fleeing, _fixture.View.Mood);
        }
    }
}
