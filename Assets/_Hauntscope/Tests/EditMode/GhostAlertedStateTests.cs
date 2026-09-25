using Hauntscope.Gameplay.Ghosts.States;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostAlertedStateTests
    {
        private GhostFixture _fixture;
        private GhostAlertedState _state;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(1.5f, 1f, 1.5f));
            _fixture.Mover.SetTarget(new Vector3(-1.5f, 1f, -1.5f));
            _state = new GhostAlertedState(_fixture.Context);
        }

        [Test]
        public void Enter_Always_FacesCameraHorizontally()
        {
            _state.Enter();

            var expected = _fixture.Camera.Position - _fixture.Mover.Position;
            expected.y = 0f;
            Assert.That(Vector3.Angle(expected, _fixture.Mover.Facing), Is.LessThan(0.01f));
        }

        [Test]
        public void Enter_Always_StopsGhost()
        {
            _state.Enter();

            Assert.AreEqual(_fixture.Mover.Position, _fixture.Mover.Target);
        }

        [Test]
        public void Tick_DuringPause_DoesNotMove()
        {
            _state.Enter();
            var before = _fixture.Mover.Position;

            _state.Tick(GhostFixture.AlertedPause * 0.5f);

            Assert.AreEqual(before, _fixture.Mover.Position);
        }

        [Test]
        public void Tick_PauseOver_StartsWanderingAgain()
        {
            _state.Enter();
            _fixture.Random.DefaultValue = 0f;

            _state.Tick(GhostFixture.AlertedPause);

            Assert.AreNotEqual(_fixture.Mover.Position, _fixture.Mover.Target);
        }
    }
}
