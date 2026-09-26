using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ToolbeltTests
    {
        private const float LensDrain = 2f;
        private const float BeamDrain = 3f;

        private GhostFixture _fixture;
        private GhostLens _lens;
        private CaptureBeam _beam;
        private Toolbelt _toolbelt;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            var session = new HuntSession();
            session.Begin(_fixture.Ghost, null);
            var config = TestConfigs.Tools(lensDrain: LensDrain, beamDrain: BeamDrain);
            _lens = new GhostLens(session, _fixture.Camera, config, new HuntModifiers());
            _beam = TestConfigs.Beam(session, _fixture.Camera, config, new HuntModifiers());
            _toolbelt = new Toolbelt(_lens, _beam);
        }

        [Test]
        public void ToggleLens_WhenOff_ActivatesLens()
        {
            _toolbelt.ToggleLens();

            Assert.IsTrue(_lens.IsActive.Value);
        }

        [Test]
        public void ToggleLens_WhenOn_DeactivatesLens()
        {
            _toolbelt.ToggleLens();

            _toolbelt.ToggleLens();

            Assert.IsFalse(_lens.IsActive.Value);
        }

        [Test]
        public void StartBeam_LensOff_TurnsLensOn()
        {
            _toolbelt.StartBeam();

            Assert.IsTrue(_lens.IsActive.Value);
            Assert.IsTrue(_beam.IsActive.Value);
        }

        [Test]
        public void StopBeam_Always_KeepsLensOn()
        {
            _toolbelt.StartBeam();

            _toolbelt.StopBeam();

            Assert.IsFalse(_beam.IsActive.Value);
            Assert.IsTrue(_lens.IsActive.Value);
        }

        [Test]
        public void TotalDrainPerSecond_NoToolActive_IsZero()
        {
            Assert.AreEqual(0f, _toolbelt.TotalDrainPerSecond);
        }

        [Test]
        public void TotalDrainPerSecond_LensActive_EqualsLensDrain()
        {
            _toolbelt.ToggleLens();

            Assert.AreEqual(LensDrain, _toolbelt.TotalDrainPerSecond);
        }

        [Test]
        public void TotalDrainPerSecond_Beaming_SumsLensAndBeam()
        {
            _toolbelt.StartBeam();

            Assert.AreEqual(LensDrain + BeamDrain, _toolbelt.TotalDrainPerSecond);
        }

        [Test]
        public void DeactivateAll_ToolsActive_TurnsEverythingOff()
        {
            _toolbelt.StartBeam();

            _toolbelt.DeactivateAll();

            Assert.IsFalse(_lens.IsActive.Value);
            Assert.IsFalse(_beam.IsActive.Value);
        }

        [Test]
        public void Tick_LensInactive_StillFadesRevealedGhost()
        {
            _fixture.Ghost.SetReveal(1f);

            _toolbelt.Tick(0.4f);

            Assert.Less(_fixture.Ghost.Reveal, 1f);
        }
    }
}
