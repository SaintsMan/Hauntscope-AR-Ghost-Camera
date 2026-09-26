using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
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
            _beam = TestConfigs.Beam(session, _fixture.Camera, config, new HuntModifiers());
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
        public void Tick_ActiveRevealedInReticle_IsLocked()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();

            _beam.Tick(0.1f);

            Assert.IsTrue(_beam.IsLocked.Value);
        }

        [Test]
        public void Tick_GhostOutsideReticle_IsNotLocked()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Camera.ViewportPoint = new Vector3(0.5f + ReticleRadius + 0.05f, 0.5f, 1f);
            _beam.Activate();

            _beam.Tick(0.1f);

            Assert.IsFalse(_beam.IsLocked.Value);
        }

        [Test]
        public void Deactivate_WhileLocked_ReleasesLock()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();
            _beam.Tick(0.1f);

            _beam.Deactivate();

            Assert.IsFalse(_beam.IsLocked.Value);
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

        [Test]
        public void Tick_LaserCaptureMultiplier_ChargesFaster()
        {
            var beam = CreateBeam(new HuntModifierSet(captureRate: 1.5f));
            _fixture.Ghost.SetReveal(1f);
            beam.Activate();

            beam.Tick(1f);

            Assert.AreEqual(CaptureRate * 1.5f / GhostFixture.Resistance, beam.Progress.Value, 1e-5f);
        }

        [Test]
        public void Tick_WiderRing_HoldsAGhostJustOutsideTheStandardRing()
        {
            var beam = CreateBeam(new HuntModifierSet(reticleRadius: 1.4f));
            _fixture.Ghost.SetReveal(1f);
            _fixture.Camera.ViewportPoint = new Vector3(0.5f + ReticleRadius * 1.2f, 0.5f, 1f);
            beam.Activate();

            beam.Tick(1f);

            Assert.IsTrue(beam.IsLocked.Value);
            Assert.AreEqual(ReticleRadius * 1.4f, beam.ReticleRadius, 1e-5f);
        }

        [Test]
        public void Tick_SlowerDecay_LosesLessProgress()
        {
            var beam = CreateBeam(new HuntModifierSet(decayRate: 0.5f));
            _fixture.Ghost.SetReveal(1f);
            beam.Activate();
            beam.Tick(2f);
            _fixture.Camera.ViewportPoint = new Vector3(0.9f, 0.5f, 1f);

            beam.Tick(1f);

            Assert.AreEqual(CaptureRate * 2f - DecayRate * 0.5f, beam.Progress.Value, 1e-5f);
        }

        [Test]
        public void Tick_HiddenGhostWithoutPhase_IsNotHeld()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.SetVisible(false);
            _beam.Activate();

            _beam.Tick(1f);

            Assert.IsFalse(_beam.IsLocked.Value);
        }

        [Test]
        public void Tick_HiddenGhostWithPhase_IsStillHeld()
        {
            var beam = CreateBeam(new HuntModifierSet(locksHiddenGhosts: true));
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.SetVisible(false);
            beam.Activate();

            beam.Tick(1f);

            Assert.IsTrue(beam.IsLocked.Value);
        }

        [Test]
        public void DrainPerSecond_BeamDrainMultiplier_Scales()
        {
            var beam = CreateBeam(new HuntModifierSet(beamDrain: 0.85f));

            Assert.AreEqual(BeamDrain * 0.85f, beam.DrainPerSecond, 1e-5f);
        }

        private CaptureBeam CreateBeam(HuntModifierSet set)
        {
            var modifiers = new HuntModifiers();
            modifiers.Apply(set);
            var session = new HuntSession();
            session.Begin(_fixture.Ghost, null);
            var config = TestConfigs.Tools(beamDrain: BeamDrain, reticleRadius: ReticleRadius, captureRate: CaptureRate, decayRate: DecayRate);
            return TestConfigs.Beam(session, _fixture.Camera, config, modifiers);
        }

        [Test]
        public void Tick_StaggeredGhost_ChargesTwiceAsFast()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Stagger(5f);
            _beam.Activate();

            _beam.Tick(1f);

            Assert.AreEqual(CaptureRate * 2f / GhostFixture.Resistance, _beam.Progress.Value, 1e-5f);
        }

        [Test]
        public void Tick_GhostUpClose_ChargesFasterThanFromAfar()
        {
            var capture = TestConfigs.Capture(closeDistance: 1f, farDistance: 3f, closeCaptureMultiplier: 1.5f, farCaptureMultiplier: 0.5f);
            var beam = CreateBeam(new HuntModifierSet(), capture);
            _fixture.Ghost.SetReveal(1f);
            _fixture.Camera.Position = _fixture.Ghost.Position + Vector3.back * 0.5f;
            beam.Activate();

            beam.Tick(1f);

            Assert.AreEqual(CaptureRate * 1.5f / GhostFixture.Resistance, beam.Progress.Value, 1e-5f);
            Assert.AreEqual(0.5f, beam.GhostDistance, 1e-5f);
        }

        [Test]
        public void Tick_JumpScare_KnocksProgressBack()
        {
            var capture = TestConfigs.Capture(scareProgressLoss: 0.3f);
            var beam = CreateBeam(new HuntModifierSet(), capture);
            _fixture.Ghost.SetReveal(1f);
            beam.Activate();
            beam.Tick(2f);
            beam.Deactivate();
            _fixture.Ghost.Tick(0.01f);
            _fixture.Ghost.Scare();
            _fixture.Ghost.Tick(0.01f);
            var before = beam.Progress.Value;

            beam.Tick(0f);

            Assert.AreEqual(before - 0.3f, beam.Progress.Value, 1e-5f);
        }

        [Test]
        public void Tick_ScareContinues_KnocksBackOnlyOnce()
        {
            var capture = TestConfigs.Capture(scareProgressLoss: 0.3f);
            var beam = CreateBeam(new HuntModifierSet(), capture);
            _fixture.Ghost.SetReveal(1f);
            beam.Activate();
            beam.Tick(2f);
            beam.Deactivate();
            _fixture.Ghost.Tick(0.01f);
            _fixture.Ghost.Scare();
            _fixture.Ghost.Tick(0.01f);
            beam.Tick(0f);
            var afterFirst = beam.Progress.Value;

            beam.Tick(0f);

            Assert.AreEqual(afterFirst, beam.Progress.Value, 1e-5f);
        }

        private CaptureBeam CreateBeam(HuntModifierSet set, Hauntscope.Gameplay.Config.CaptureConfig capture)
        {
            var modifiers = new HuntModifiers();
            modifiers.Apply(set);
            var session = new HuntSession();
            session.Begin(_fixture.Ghost, null);
            var config = TestConfigs.Tools(beamDrain: BeamDrain, reticleRadius: ReticleRadius, captureRate: CaptureRate, decayRate: DecayRate);
            return TestConfigs.Beam(session, _fixture.Camera, config, modifiers, capture);
        }

        [Test]
        public void Tick_YankLeftTheRingEmpty_TearsTheCaptureBack()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();
            _beam.Tick(2f);
            _fixture.Ghost.TestGrip(0.25f);
            _fixture.Camera.ViewportPoint = new Vector3(0.5f + ReticleRadius + 0.05f, 0.5f, 1f);

            _beam.Tick(1f);

            Assert.AreEqual(CaptureRate * 2f - 0.25f - DecayRate, _beam.Progress.Value, 1e-5f);
        }

        [Test]
        public void Tick_YankButStillInTheRing_KeepsTheCapture()
        {
            _fixture.Ghost.SetReveal(1f);
            _beam.Activate();
            _beam.Tick(2f);
            _fixture.Ghost.TestGrip(0.25f);

            _beam.Tick(1f);

            Assert.AreEqual(CaptureRate * 3f, _beam.Progress.Value, 1e-5f);
        }
    }
}
