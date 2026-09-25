using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostLensTests
    {
        private const float LensDrain = 2f;
        private const float RevealAngle = 35f;
        private const float RevealInTime = 0.4f;
        private const float RevealOutTime = 0.8f;

        private GhostFixture _fixture;
        private HuntSession _session;
        private GhostLens _lens;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Camera.Position = new Vector3(0f, 1f, -2f);
            _fixture.Camera.Forward = Vector3.forward;
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            _lens = new GhostLens(_session, _fixture.Camera, TestConfigs.Tools(lensDrain: LensDrain, revealAngle: RevealAngle, revealInTime: RevealInTime, revealOutTime: RevealOutTime));
        }

        [Test]
        public void Activate_Always_BecomesActive()
        {
            _lens.Activate();

            Assert.IsTrue(_lens.IsActive.Value);
        }

        [Test]
        public void DrainPerSecond_Always_EqualsLensDrain()
        {
            Assert.AreEqual(LensDrain, _lens.DrainPerSecond);
        }

        [Test]
        public void Tick_ActiveAndGhostInView_RevealsFullyAfterRevealInTime()
        {
            _lens.Activate();

            _lens.Tick(RevealInTime);

            Assert.AreEqual(1f, _fixture.Ghost.Reveal, 1e-5f);
        }

        [Test]
        public void Tick_ActiveAndGhostInView_RevealsGradually()
        {
            _lens.Activate();

            _lens.Tick(RevealInTime / 2f);

            Assert.AreEqual(0.5f, _fixture.Ghost.Reveal, 1e-5f);
        }

        [Test]
        public void Tick_GhostOutsideRevealAngle_StaysHidden()
        {
            _fixture.Camera.Forward = Vector3.right;
            _lens.Activate();

            _lens.Tick(RevealInTime);

            Assert.AreEqual(0f, _fixture.Ghost.Reveal);
        }

        [Test]
        public void Tick_GhostBeyondRevealRange_StaysHidden()
        {
            _fixture.Camera.Position = new Vector3(0f, 1f, -GhostFixture.RevealRange - 0.5f);
            _lens.Activate();

            _lens.Tick(RevealInTime);

            Assert.AreEqual(0f, _fixture.Ghost.Reveal);
        }

        [Test]
        public void Tick_LensTurnedOff_FadesOutOverRevealOutTime()
        {
            _fixture.Ghost.SetReveal(1f);

            _lens.Tick(RevealOutTime / 2f);

            Assert.AreEqual(0.5f, _fixture.Ghost.Reveal, 1e-5f);
        }

        [Test]
        public void Tick_NoGhostInSession_DoesNothing()
        {
            var lens = new GhostLens(new HuntSession(), _fixture.Camera, TestConfigs.Tools(lensDrain: LensDrain, revealAngle: RevealAngle, revealInTime: RevealInTime, revealOutTime: RevealOutTime));
            lens.Activate();

            Assert.DoesNotThrow(() => lens.Tick(0.1f));
        }
    }
}
