using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ToolbeltTests
    {
        private const float LensDrain = 2f;

        private GhostFixture _fixture;
        private GhostLens _lens;
        private Toolbelt _toolbelt;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            var session = new HuntSession();
            session.SetGhost(_fixture.Ghost);
            _lens = new GhostLens(session, _fixture.Camera, new ToolsConfig(LensDrain, 35f, 0.4f, 0.8f));
            _toolbelt = new Toolbelt(_lens);
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
        public void DeactivateAll_LensActive_TurnsLensOff()
        {
            _toolbelt.ToggleLens();

            _toolbelt.DeactivateAll();

            Assert.IsFalse(_lens.IsActive.Value);
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
