using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostScareStateTests
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
        public void Scare_WhenAlerted_StartsScaring()
        {
            MakeAlerted();

            _fixture.Ghost.Scare();
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsScaring);
        }

        [Test]
        public void Scare_WhenWandering_IsIgnored()
        {
            _fixture.Ghost.Scare();
            _fixture.Ghost.Tick(0.01f);

            Assert.IsFalse(_fixture.Ghost.IsScaring);
        }

        [Test]
        public void Scare_BeamedOnSameTick_ScareWinsOverFleeing()
        {
            MakeAlerted();
            _fixture.Ghost.SetBeamed(true);

            _fixture.Ghost.Scare();
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsScaring);
        }

        [Test]
        public void Scaring_AfterRushTime_IsInFrontOfCamera()
        {
            StartScare();

            _fixture.Ghost.Tick(GhostFixture.ScareRushTime);

            var face = _fixture.Camera.Position + _fixture.Camera.Forward * GhostFixture.ScareFaceDistance;
            Assert.Less(Vector3.Distance(_fixture.Mover.Position, face), 0.05f);
        }

        [Test]
        public void Scaring_WhileLensHidesGhost_IsFullyVisible()
        {
            StartScare();
            _fixture.Ghost.SetReveal(0f);
            _fixture.Ghost.SetVisible(false);

            _fixture.Ghost.Tick(0.01f);

            Assert.AreEqual(1f, _fixture.View.Reveal);
        }

        [Test]
        public void Scaring_AfterDuration_ReturnsToAlertedAtStartPosition()
        {
            StartScare();
            var start = _fixture.Mover.Position;

            _fixture.Ghost.Tick(GhostFixture.ScareDuration);
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsAlerted);
            Assert.Less(Vector3.Distance(_fixture.Mover.Position, start), 0.05f);
        }

        [Test]
        public void TeleportTo_Always_RaisesTeleportedWithBothPositions()
        {
            var from = _fixture.Mover.Position;
            var to = new Vector3(1f, 1f, 1f);
            var raisedFrom = Vector3.zero;
            var raisedTo = Vector3.zero;
            _fixture.Ghost.Teleported += (a, b) =>
            {
                raisedFrom = a;
                raisedTo = b;
            };

            _fixture.Ghost.TeleportTo(to);

            Assert.AreEqual(from, raisedFrom);
            Assert.AreEqual(_fixture.Mover.Position, raisedTo);
        }

        private void MakeAlerted()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Tick(0.01f);
        }

        private void StartScare()
        {
            MakeAlerted();
            _fixture.Ghost.Scare();
            _fixture.Ghost.Tick(0.01f);
        }
    }
}
