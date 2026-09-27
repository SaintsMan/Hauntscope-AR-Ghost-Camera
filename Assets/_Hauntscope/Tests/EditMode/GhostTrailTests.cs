using Hauntscope.Gameplay.Tools;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostTrailTests
    {
        private const float Spacing = 0.5f;

        private GhostTrail _trail;

        [SetUp]
        public void SetUp()
        {
            _trail = new GhostTrail(4, Spacing);
        }

        [Test]
        public void Follow_FirstPosition_LeavesNoDripYet()
        {
            _trail.Follow(Vector3.zero, 1f);

            Assert.AreEqual(0, CountMarks());
        }

        [Test]
        public void Follow_MovedLessThanSpacing_LeavesNothing()
        {
            _trail.Follow(Vector3.zero, 1f);

            _trail.Follow(new Vector3(0.3f, 0f, 0f), 2f);

            Assert.AreEqual(0, CountMarks());
        }

        [Test]
        public void Follow_MovedASpacing_LeavesADripHeadingThatWay()
        {
            _trail.Follow(Vector3.zero, 1f);

            _trail.Follow(new Vector3(0.6f, 0f, 0f), 2f);

            Assert.AreEqual(1, CountMarks());
            Assert.AreEqual(TrailMarkKind.Drip, _trail[0].Kind);
            Assert.AreEqual(90f, _trail[0].Yaw, 1e-3f);
        }

        [Test]
        public void Mark_Always_AddsThatKind()
        {
            _trail.Mark(Vector3.one, TrailMarkKind.Handprint, 1f);

            Assert.AreEqual(TrailMarkKind.Handprint, _trail[0].Kind);
            Assert.AreEqual(Vector3.one, _trail[0].Position);
        }

        [Test]
        public void Mark_RingFull_OverwritesTheOldest()
        {
            for (var i = 0; i < 5; i++)
                _trail.Mark(new Vector3(i, 0f, 0f), TrailMarkKind.Splash, i + 1f);

            Assert.AreEqual(4f, _trail[0].Position.x);
            Assert.AreEqual(4, CountMarks());
        }

        [Test]
        public void Restart_AfterAJump_NextDripStartsFromTheLanding()
        {
            _trail.Follow(Vector3.zero, 1f);
            _trail.Restart(new Vector3(5f, 0f, 0f));

            _trail.Follow(new Vector3(5.2f, 0f, 0f), 2f);

            Assert.AreEqual(0, CountMarks());
        }

        [Test]
        public void Clear_WithMarks_EmptiesEverySlot()
        {
            _trail.Mark(Vector3.one, TrailMarkKind.Splash, 1f);

            _trail.Clear();

            Assert.AreEqual(0, CountMarks());
        }

        private int CountMarks()
        {
            var count = 0;
            for (var i = 0; i < _trail.Capacity; i++)
            {
                if (!_trail[i].IsEmpty)
                    count++;
            }

            return count;
        }
    }
}
