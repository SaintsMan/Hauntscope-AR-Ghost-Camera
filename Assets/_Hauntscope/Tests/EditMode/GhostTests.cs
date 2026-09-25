using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostTests
    {
        private GhostMover _mover;
        private FakeGhostView _view;
        private Ghost _ghost;

        [SetUp]
        public void SetUp()
        {
            var planes = new FakePlaneProvider { RoomBounds = new Bounds(Vector3.zero, new Vector3(4f, 0f, 4f)) };
            var config = new GhostConfig(3f, 6f, 0.5f, 0.05f, 0.5f);
            _mover = new GhostMover(planes, config);
            _mover.Teleport(new Vector3(1f, 1f, 1f));
            var context = new GhostContext(new GhostMotion(1f, 1f, 2f), config, _mover, new FakeRandom(), planes);
            _view = new FakeGhostView();
            _ghost = new Ghost(context, _view);
        }

        [Test]
        public void Start_Always_PlacesViewAtGhost()
        {
            _ghost.Start();

            Assert.AreEqual(_mover.VisualPosition, _view.Position);
        }

        [Test]
        public void Tick_AfterStart_MovesViewWithGhost()
        {
            _ghost.Start();

            _ghost.Tick(0.1f);

            Assert.AreEqual(_mover.VisualPosition, _view.Position);
            Assert.AreEqual(2, _view.SetPositionCount);
        }
    }
}
