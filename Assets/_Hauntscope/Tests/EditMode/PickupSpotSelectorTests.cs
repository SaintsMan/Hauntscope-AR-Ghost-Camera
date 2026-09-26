using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Pickups;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PickupSpotSelectorTests
    {
        private FakePlaneProvider _planes;
        private FakeRandom _random;
        private PickupSpotSelector _selector;

        [SetUp]
        public void SetUp()
        {
            _planes = new FakePlaneProvider { RoomBounds = new Bounds(Vector3.zero, new Vector3(10f, 0f, 10f)), FloorHeight = -0.1f };
            _random = new FakeRandom();
            var config = new PickupConfig(new PickupSpawn[0], spawnMinDistance: 1.5f, spawnMaxDistance: 4f, minSpacing: 1f);
            _selector = new PickupSpotSelector(_planes, _random, config);
        }

        [Test]
        public void TrySelect_FreeFloorInRange_ReturnsItAtFloorHeight()
        {
            _random.Enqueue(0.8f, 0.5f);

            var found = _selector.TrySelect(Vector3.zero, new List<Pickup>(), out var position);

            Assert.IsTrue(found);
            Assert.AreEqual(new Vector3(3f, -0.1f, 0f), position);
        }

        [Test]
        public void TrySelect_TooCloseToThePlayer_SkipsTheCandidate()
        {
            _random.Enqueue(0.55f, 0.5f, 0.8f, 0.5f);

            _selector.TrySelect(Vector3.zero, new List<Pickup>(), out var position);

            Assert.AreEqual(3f, position.x, 1e-4f);
        }

        [Test]
        public void TrySelect_OnFurnitureOrTable_SkipsTheCandidate()
        {
            _planes.FloorTest = point => point.x < 0f;
            _random.Enqueue(0.8f, 0.5f, 0.2f, 0.5f);

            _selector.TrySelect(Vector3.zero, new List<Pickup>(), out var position);

            Assert.AreEqual(-3f, position.x, 1e-4f);
        }

        [Test]
        public void TrySelect_NextToAnotherPickup_SkipsTheCandidate()
        {
            var taken = new List<Pickup> { new Pickup(null, null, new FakePickupView(), new Vector3(3f, 0f, 0.3f)) };
            _random.Enqueue(0.8f, 0.5f, 0.2f, 0.5f);

            _selector.TrySelect(Vector3.zero, taken, out var position);

            Assert.AreEqual(-3f, position.x, 1e-4f);
        }

        [Test]
        public void TrySelect_NoFloorAtAll_ReturnsFalse()
        {
            _planes.FloorTest = _ => false;

            var found = _selector.TrySelect(Vector3.zero, new List<Pickup>(), out _);

            Assert.IsFalse(found);
        }
    }
}
