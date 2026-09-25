using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostTransitionsTests
    {
        private GhostFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            _fixture.Ghost.Start();
        }

        [Test]
        public void Alerted_Beamed_StartsFleeing()
        {
            MakeAlerted();
            _fixture.Ghost.SetBeamed(true);

            _fixture.Ghost.Tick(0.1f);

            Assert.IsTrue(_fixture.Ghost.IsFleeing);
        }

        [Test]
        public void Fleeing_NoBeamForCalmDownTime_ReturnsToAlerted()
        {
            MakeFleeing();
            _fixture.Ghost.SetBeamed(false);

            _fixture.Ghost.Tick(GhostFixture.FleeCalmDownTime);
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsAlerted);
        }

        [Test]
        public void Fleeing_StillBeamed_KeepsFleeing()
        {
            MakeFleeing();

            _fixture.Ghost.Tick(GhostFixture.FleeCalmDownTime);
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsFleeing);
        }

        [Test]
        public void Wandering_Captured_GoesStraightToCaptured()
        {
            _fixture.Ghost.Capture();

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsCaptured);
        }

        [Test]
        public void Fleeing_Captured_GoesToCaptured()
        {
            MakeFleeing();
            _fixture.Ghost.Capture();

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsCaptured);
        }

        [Test]
        public void Captured_LaterTicks_StaysCaptured()
        {
            _fixture.Ghost.Capture();

            for (var i = 0; i < 10; i++)
                _fixture.Ghost.Tick(0.5f);

            Assert.IsTrue(_fixture.Ghost.IsCaptured);
        }

        [Test]
        public void Captured_LensFadesReveal_StaysFullyRevealed()
        {
            _fixture.Ghost.Capture();

            _fixture.Ghost.SetReveal(0f);

            Assert.AreEqual(1f, _fixture.Ghost.Reveal);
        }

        [Test]
        public void Captured_AfterCaptureDuration_FinishesDissolving()
        {
            _fixture.Ghost.Capture();
            _fixture.Ghost.Tick(0.01f);

            _fixture.Ghost.Tick(GhostFixture.CaptureDuration);

            Assert.IsTrue(_fixture.Ghost.IsCaptureFinished);
            Assert.AreEqual(1f, _fixture.View.Dissolve, 1e-5f);
        }

        [Test]
        public void Captured_WhileDissolving_MovesTowardsCamera()
        {
            var before = Vector3.Distance(_fixture.Mover.Position, _fixture.Camera.Position);
            _fixture.Ghost.Capture();
            _fixture.Ghost.Tick(0.01f);

            _fixture.Ghost.Tick(GhostFixture.CaptureDuration * 0.9f);

            Assert.Less(Vector3.Distance(_fixture.Mover.Position, _fixture.Camera.Position), before);
        }

        [Test]
        public void Fleeing_Escape_GoesToEscaped()
        {
            MakeFleeing();
            _fixture.Ghost.Escape();

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsEscaped);
        }

        [Test]
        public void Escaped_AfterEscapeDuration_FadesOutCompletely()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Escape();
            _fixture.Ghost.Tick(0.01f);

            _fixture.Ghost.Tick(GhostFixture.EscapeDuration);

            Assert.IsTrue(_fixture.Ghost.IsEscapeFinished);
            Assert.AreEqual(0f, _fixture.View.Reveal, 1e-5f);
        }

        [Test]
        public void Captured_ThenEscapeRequested_StaysCaptured()
        {
            _fixture.Ghost.Capture();
            _fixture.Ghost.Escape();

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsCaptured);
        }

        [Test]
        public void Escaped_ThenCaptureRequested_StaysEscaped()
        {
            _fixture.Ghost.Escape();
            _fixture.Ghost.Capture();

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsEscaped);
        }

        private void MakeAlerted()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Tick(0.01f);
        }

        private void MakeFleeing()
        {
            MakeAlerted();
            _fixture.Ghost.SetBeamed(true);
            _fixture.Ghost.Tick(0.01f);
        }
    }
}
