using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ToolbeltTests
    {
        private const float LensDrain = 2f;
        private const float BeamDrain = 3f;
        private const float BoxDrain = 0.5f;

        private GhostFixture _fixture;
        private GhostLens _lens;
        private CaptureBeam _beam;
        private SpiritBox _box;
        private FakeViewMode _mode;
        private PlayerProgress _progress;
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
            _progress = new PlayerProgress();
            _progress.AddCapture("wisp", 10);
            _box = TestConfigs.SpiritBoxTool(session, _fixture.Camera, config, new FakeRandom(), _progress, TestConfigs.SpiritBox(drain: BoxDrain));
            _mode = new FakeViewMode { DrainPerSecond = 0.7f };
            _toolbelt = new Toolbelt(_lens, _beam, _box, TestConfigs.Views(_mode));
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
        public void ToggleSpiritBox_Unlocked_ActivatesBox()
        {
            _toolbelt.ToggleSpiritBox();

            Assert.IsTrue(_box.IsActive.Value);
        }

        [Test]
        public void ToggleSpiritBox_WhenOn_DeactivatesBox()
        {
            _toolbelt.ToggleSpiritBox();

            _toolbelt.ToggleSpiritBox();

            Assert.IsFalse(_box.IsActive.Value);
        }

        [Test]
        public void TotalDrainPerSecond_SpiritBoxAndLens_SumsBoth()
        {
            _toolbelt.ToggleSpiritBox();
            _toolbelt.ToggleLens();

            Assert.AreEqual(LensDrain + BoxDrain, _toolbelt.TotalDrainPerSecond, 1e-5f);
        }

        [Test]
        public void DeactivateAll_SpiritBoxOn_TurnsItOff()
        {
            _toolbelt.ToggleSpiritBox();

            _toolbelt.DeactivateAll();

            Assert.IsFalse(_box.IsActive.Value);
        }

        [Test]
        public void CycleView_ModeIssued_AddsItsDrain()
        {
            _toolbelt.CycleView();

            Assert.AreEqual(0.7f, _toolbelt.TotalDrainPerSecond, 1e-5f);
        }

        [Test]
        public void DeactivateAll_ModeOn_BackToPlainPicture()
        {
            _toolbelt.CycleView();

            _toolbelt.DeactivateAll();

            Assert.IsNull(_toolbelt.Views.Current.Value);
            Assert.IsFalse(_mode.IsActive.Value);
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
