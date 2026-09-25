using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Ghosts.States;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostWanderStateTests
    {
        private const float WanderInterval = 4f;
        private const float FloorHeight = 0.2f;

        private FakePlaneProvider _planes;
        private FakeRandom _random;
        private GhostMover _mover;
        private GhostWanderState _state;

        [SetUp]
        public void SetUp()
        {
            _planes = new FakePlaneProvider
            {
                RoomBounds = new Bounds(Vector3.zero, new Vector3(4f, 0f, 4f)),
                FloorHeight = FloorHeight
            };
            _random = new FakeRandom();
            var config = new GhostConfig(WanderInterval, WanderInterval, 0.5f, 0.05f, 0.5f);
            _mover = new GhostMover(_planes, config);
            var context = new GhostContext(new GhostMotion(1f, 1f, 2f), config, _mover, _random, _planes);
            _state = new GhostWanderState(context);
        }

        [Test]
        public void Enter_RandomAtMiddle_TargetsRoomCenterAtMiddleHoverHeight()
        {
            _state.Enter();

            Assert.AreEqual(new Vector3(0f, FloorHeight + 1.5f, 0f), _mover.Target);
        }

        [Test]
        public void Tick_BeforeInterval_KeepsTarget()
        {
            _state.Enter();
            var target = _mover.Target;
            _random.DefaultValue = 1f;

            _state.Tick(WanderInterval - 0.1f);

            Assert.AreEqual(target, _mover.Target);
        }

        [Test]
        public void Tick_IntervalElapsed_PicksNewTarget()
        {
            _state.Enter();
            _random.DefaultValue = 1f;

            _state.Tick(WanderInterval);

            Assert.AreEqual(new Vector3(2f, FloorHeight + 2f, 2f), _mover.Target);
        }

        [Test]
        public void Tick_Always_MovesGhostTowardsTarget()
        {
            _mover.Teleport(new Vector3(-2f, 1f, -2f));
            _state.Enter();
            var before = _mover.Position;

            _state.Tick(0.1f);

            Assert.AreNotEqual(before, _mover.Position);
        }
    }
}
