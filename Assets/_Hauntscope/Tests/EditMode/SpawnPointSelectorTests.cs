using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SpawnPointSelectorTests
    {
        private const float FloorHeight = 0.1f;

        private FakePlaneProvider _planes;
        private FakeRandom _random;
        private SpawnPointSelector _selector;

        [SetUp]
        public void SetUp()
        {
            _planes = new FakePlaneProvider
            {
                RoomBounds = new Bounds(Vector3.zero, new Vector3(10f, 0f, 10f)),
                FloorHeight = FloorHeight
            };
            _random = new FakeRandom();
            _selector = new SpawnPointSelector(_planes, _random, new RoomConfig(1.5f, 1.5f, 0.3f, 2f, 5f));
        }

        [Test]
        public void Select_CandidateWithinDistanceRange_ReturnsIt()
        {
            _random.Enqueue(0.8f, 0.5f);

            var point = _selector.Select(Vector3.zero);

            Assert.AreEqual(new Vector3(3f, FloorHeight, 0f), point);
        }

        [Test]
        public void Select_FirstCandidateTooClose_TriesNextOne()
        {
            _random.Enqueue(0.5f, 0.5f, 0.8f, 0.5f);

            var point = _selector.Select(Vector3.zero);

            Assert.AreEqual(new Vector3(3f, FloorHeight, 0f), point);
        }

        [Test]
        public void Select_IgnoresHeightDifferenceFromOrigin()
        {
            _random.Enqueue(0.8f, 0.5f);

            var point = _selector.Select(new Vector3(0f, 1.6f, 0f));

            Assert.AreEqual(new Vector3(3f, FloorHeight, 0f), point);
        }

        [Test]
        public void Select_NoCandidateInRange_ReturnsFarthestCorner()
        {
            _random.DefaultValue = 0.9f;

            var point = _selector.Select(new Vector3(4f, 1.5f, 4f));

            Assert.AreEqual(new Vector3(-5f, FloorHeight, -5f), point);
        }
    }
}
