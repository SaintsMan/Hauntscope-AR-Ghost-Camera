using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class CaptureBeamTests
    {
        private const float BeamDrain = 3f;
        private const float ReticleRadius = 0.18f;
        private const float CaptureRate = 0.2f;
        private const float DecayRate = 0.1f;

        private GhostFixture _fixture;
        private CaptureBeam _beam;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
            var session = new HuntSession();
            session.Begin(_fixture.Ghost, null);
            var config = TestConfigs.Tools(beamDrain: BeamDrain, reticleRadius: ReticleRadius, captureRate: CaptureRate, decayRate: DecayRate);
            _beam = new CaptureBeam(session, _fixture.Camera, config);
        }

        [Test]
        public void DrainPerSecond_Always_EqualsBeamDrain()
        {
            Assert.AreEqual(BeamDrain, _beam.DrainPerSecond);
        }

        [Test]
        public void Tick_ActiveRevealedInReticle_GrowsByRateOverResistance()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();

            _beam.Tick(1f);

            Assert.AreEqual(CaptureRate / GhostFixture.Resistance, _beam.Progress.Value, 1e-5f);
        }

        [Test]
        public void Tick_GhostNotRevealedEnough_DoesNotGrow()
        {
            _fixture.Ghost.SetReveal(0.4f);
            _beam.Activate();

            _beam.Tick(1f);

            Assert.AreEqual(0f, _beam.Progress.Value);
        }

        [Test]
        public void Tick_GhostOutsideReticle_Decays()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();
            _beam.Tick(2f);
            _fixture.Camera.ViewportPoint = new Vector3(0.5f + ReticleRadius + 0.05f, 0.5f, 1f);

            _beam.Tick(1f);

            Assert.AreEqual(CaptureRate * 2f - DecayRate, _beam.Progress.Value, 1e-5f);
        }

        [Test]
        public void Tick_VerticalOffsetWithinRadiusInWidthUnits_CountsAsInReticle()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Camera.Aspect = 0.5f;
            _fixture.Camera.ViewportPoint = new Vector3(0.5f, 0.5f + ReticleRadius * 0.5f * 0.9f, 1f);
            _beam.Activate();

            _beam.Tick(1f);

            Assert.Greater(_beam.Progress.Value, 0f);
        }

        [Test]
        public void Tick_GhostBehindCamera_IsNotInReticle()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Camera.ViewportPoint = new Vector3(0.5f, 0.5f, -1f);
            _beam.Activate();

            _beam.Tick(1f);

            Assert.AreEqual(0f, _beam.Progress.Value);
        }

        [Test]
        public void Tick_Inactive_NeverDropsBelowZero()
        {
            _beam.Tick(5f);

            Assert.AreEqual(0f, _beam.Progress.Value);
        }

        [Test]
        public void Tick_ActiveOnRevealedGhost_MarksGhostBeamed()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();

            _beam.Tick(0.1f);

            Assert.IsTrue(_fixture.Ghost.IsBeamed);
        }

        [Test]
        public void Tick_Released_ClearsBeamedFlag()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();
            _beam.Tick(0.1f);
            _beam.Deactivate();

            _beam.Tick(0.1f);

            Assert.IsFalse(_fixture.Ghost.IsBeamed);
        }

        [Test]
        public void Tick_ProgressReachesFull_CapturesGhost()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();

            _beam.Tick(GhostFixture.Resistance / CaptureRate);
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsCaptured);
        }

        [Test]
        public void ResetProgress_AfterBeaming_SetsZero()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();
            _beam.Tick(1f);

            _beam.ResetProgress();

            Assert.AreEqual(0f, _beam.Progress.Value);
        }
    }
}
