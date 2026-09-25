using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostMoverTests
    {
        private const float MaxSpeed = 1f;
        private const float DeltaTime = 0.02f;
        private const float BobAmplitude = 0.05f;

        private FakePlaneProvider _planes;
        private GhostMover _mover;

        [SetUp]
        public void SetUp()
        {
            _planes = new FakePlaneProvider { RoomBounds = new Bounds(Vector3.zero, new Vector3(4f, 0f, 4f)) };
            _mover = new GhostMover(_planes, GhostFixture.CreateConfig());
        }

        [Test]
        public void Teleport_OutsideRoom_ClampsIntoRoomBounds()
        {
            _mover.Teleport(new Vector3(10f, 1f, -10f));

            Assert.AreEqual(new Vector3(2f, 1f, -2f), _mover.Position);
        }

        [Test]
        public void Teleport_Always_ResetsTargetToPosition()
        {
            _mover.SetTarget(new Vector3(1f, 1f, 1f));

            _mover.Teleport(new Vector3(-1f, 1f, 0f));

            Assert.AreEqual(_mover.Position, _mover.Target);
        }

        [Test]
        public void SetTarget_OutsideRoom_ClampsHorizontallyAndKeepsHeight()
        {
            _mover.SetTarget(new Vector3(-7f, 1.5f, 3f));

            Assert.AreEqual(new Vector3(-2f, 1.5f, 2f), _mover.Target);
        }

        [Test]
        public void Tick_TargetAhead_MovesCloser()
        {
            _mover.Teleport(new Vector3(-1f, 1f, 0f));
            _mover.SetTarget(new Vector3(1f, 1f, 0f));
            var distanceBefore = Vector3.Distance(_mover.Position, _mover.Target);

            _mover.Tick(DeltaTime, MaxSpeed);

            Assert.Less(Vector3.Distance(_mover.Position, _mover.Target), distanceBefore);
        }

        [Test]
        public void Tick_OneStep_DoesNotExceedMaxSpeed()
        {
            _mover.Teleport(new Vector3(-2f, 1f, -2f));
            _mover.SetTarget(new Vector3(2f, 1f, 2f));
            var before = _mover.Position;

            _mover.Tick(DeltaTime, MaxSpeed);

            Assert.LessOrEqual(Vector3.Distance(before, _mover.Position), MaxSpeed * DeltaTime + 1e-4f);
        }

        [Test]
        public void Tick_ManySteps_ReachesTarget()
        {
            _mover.Teleport(new Vector3(-1f, 1f, -1f));
            _mover.SetTarget(new Vector3(1f, 1.2f, 1f));

            for (var i = 0; i < 1000; i++)
                _mover.Tick(DeltaTime, MaxSpeed);

            Assert.Less(Vector3.Distance(_mover.Position, _mover.Target), 0.01f);
        }

        [Test]
        public void Tick_TargetOnRoomEdge_NeverLeavesRoomBounds()
        {
            _mover.Teleport(new Vector3(-2f, 1f, -2f));
            _mover.SetTarget(new Vector3(2f, 1f, 2f));
            var leftRoom = false;

            for (var i = 0; i < 500; i++)
            {
                _mover.Tick(DeltaTime, MaxSpeed * 3f);
                var position = _mover.Position;
                leftRoom |= Mathf.Abs(position.x) > 2f || Mathf.Abs(position.z) > 2f;
            }

            Assert.IsFalse(leftRoom);
        }

        [Test]
        public void VisualPosition_WhileBobbing_StaysWithinAmplitude()
        {
            _mover.Teleport(new Vector3(0f, 1f, 0f));
            var maxOffset = 0f;

            for (var i = 0; i < 200; i++)
            {
                _mover.Tick(DeltaTime, MaxSpeed);
                maxOffset = Mathf.Max(maxOffset, Mathf.Abs(_mover.VisualPosition.y - _mover.Position.y));
            }

            Assert.LessOrEqual(maxOffset, BobAmplitude + 1e-5f);
            Assert.Greater(maxOffset, 0f);
        }
    }
}
