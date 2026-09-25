using Hauntscope.Gameplay.Ghosts.States;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostWanderStateTests
    {
        private const float WanderInterval = 4f;
        private const float FloorHeight = 0.2f;

        private GhostFixture _fixture;
        private GhostWanderState _state;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture(WanderInterval, FloorHeight);
            _state = new GhostWanderState(_fixture.Context, 1f);
        }

        [Test]
        public void Enter_RandomAtMiddle_TargetsRoomCenterAtMiddleHoverHeight()
        {
            _state.Enter();

            Assert.AreEqual(new Vector3(0f, FloorHeight + 1.5f, 0f), _fixture.Mover.Target);
        }

        [Test]
        public void Tick_BeforeInterval_KeepsTarget()
        {
            _state.Enter();
            var target = _fixture.Mover.Target;
            _fixture.Random.DefaultValue = 1f;

            _state.Tick(WanderInterval - 0.1f);

            Assert.AreEqual(target, _fixture.Mover.Target);
        }

        [Test]
        public void Tick_IntervalElapsed_PicksNewTarget()
        {
            _state.Enter();
            _fixture.Random.DefaultValue = 1f;

            _state.Tick(WanderInterval);

            Assert.AreEqual(new Vector3(2f, FloorHeight + 2f, 2f), _fixture.Mover.Target);
        }

        [Test]
        public void Tick_Always_MovesGhostTowardsTarget()
        {
            _fixture.Mover.Teleport(new Vector3(-2f, 1f, -2f));
            _state.Enter();
            var before = _fixture.Mover.Position;

            _state.Tick(0.1f);

            Assert.AreNotEqual(before, _fixture.Mover.Position);
        }

        [Test]
        public void Tick_WithSpeedMultiplier_MovesFartherThanNormal()
        {
            var fast = new GhostFixture(WanderInterval, FloorHeight);
            var fastState = new GhostWanderState(fast.Context, 3f);
            _fixture.Mover.Teleport(new Vector3(-2f, 1f, -2f));
            fast.Mover.Teleport(new Vector3(-2f, 1f, -2f));
            _state.Enter();
            fastState.Enter();

            for (var i = 0; i < 50; i++)
            {
                _state.Tick(0.05f);
                fastState.Tick(0.05f);
            }

            Assert.Greater(
                Vector3.Distance(new Vector3(-2f, 1f, -2f), fast.Mover.Position),
                Vector3.Distance(new Vector3(-2f, 1f, -2f), _fixture.Mover.Position));
        }
    }
}
