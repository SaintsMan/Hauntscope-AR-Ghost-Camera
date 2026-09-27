using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PrankPhotoModeTests
    {
        private const float Hover = 1f;
        private const float MinDistance = 0.8f;
        private const float MaxDistance = 4f;
        private const float AimDistance = 1.8f;

        private GhostData _wisp;
        private GhostData _shade;
        private GhostData _banshee;
        private PlayerProgress _progress;
        private FakeCameraPose _camera;
        private FakeGhostViewSpawner _spawner;
        private FakePhotoStorage _storage;
        private FakePhotoCapture _capture;
        private PrankPhotoMode _prank;

        [SetUp]
        public void SetUp()
        {
            _wisp = Ghost("wisp");
            _shade = Ghost("shade");
            _banshee = Ghost("banshee");
            _progress = new PlayerProgress();
            _progress.AddCapture("wisp", 10);
            _progress.AddCapture("banshee", 10);
            _camera = new FakeCameraPose { Position = new Vector3(0f, 1.5f, 0f), Forward = new Vector3(0f, -0.6f, 0.8f) };
            _spawner = new FakeGhostViewSpawner();
            _storage = new FakePhotoStorage();
            _capture = new FakePhotoCapture(_storage);
            _prank = new PrankPhotoMode(_progress, new GhostConfig(new[] { _wisp, _shade, _banshee }, "wisp", 1f, 1f, 1f), _camera,
                new FakePlaneProvider(), _spawner, _capture, _storage,
                new PrankPhotoConfig(1, MinDistance, MaxDistance, AimDistance, 0.5f, 1.6f, 2f), new FakeClock());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_wisp);
            Object.DestroyImmediate(_shade);
            Object.DestroyImmediate(_banshee);
        }

        [Test]
        public void IsUnlocked_NoCaptures_IsFalse()
        {
            var prank = new PrankPhotoMode(new PlayerProgress(), new GhostConfig(), _camera, new FakePlaneProvider(), _spawner, _capture, _storage,
                new PrankPhotoConfig(), new FakeClock());

            Assert.IsFalse(prank.IsUnlocked);
        }

        [Test]
        public void Begin_Always_OffersOnlyCaughtGhostsInBestiaryOrder()
        {
            _prank.Begin();

            CollectionAssert.AreEqual(new[] { _wisp, _banshee }, _prank.Roster);
        }

        [Test]
        public void Begin_GhostsCaught_PosesTheFirstFullyRevealed()
        {
            _prank.Begin();

            Assert.AreEqual(_wisp, _prank.Ghost.Value);
            Assert.AreEqual(PrankPhase.Aiming, _prank.Phase.Value);
            Assert.AreEqual(1f, _spawner.Last.Reveal);
        }

        [Test]
        public void Tick_Aiming_StandsWhereTheCameraLooksAtTheFloor()
        {
            _prank.Begin();

            _prank.Tick(0.1f);

            Assert.AreEqual(0f, _prank.FloorPoint.x, 1e-4f);
            Assert.AreEqual(2f, _prank.FloorPoint.z, 1e-4f);
            Assert.AreEqual(Hover, _spawner.Last.Position.y, 1e-4f);
        }

        [Test]
        public void Tick_Aiming_FacesTheCamera()
        {
            _prank.Begin();

            _prank.Tick(0.1f);

            Assert.AreEqual(180f, Mathf.Repeat(_prank.Yaw, 360f), 1e-3f);
        }

        [Test]
        public void Tick_LookingAboveTheHorizon_StandsAStepAhead()
        {
            _prank.Begin();
            _camera.Forward = new Vector3(1f, 0.2f, 0f).normalized;

            _prank.Tick(0.1f);

            Assert.AreEqual(AimDistance, _prank.FloorPoint.x, 1e-4f);
        }

        [Test]
        public void Tick_LookingFarAway_KeptWithinReach()
        {
            _prank.Begin();
            _camera.Forward = new Vector3(0f, -0.1f, 1f).normalized;

            _prank.Tick(0.1f);

            Assert.AreEqual(MaxDistance, _prank.FloorPoint.z, 1e-4f);
        }

        [Test]
        public void Tick_Placed_StaysPutWhenTheCameraTurns()
        {
            _prank.Begin();
            _prank.Tick(0.1f);
            _prank.Place();
            _camera.Forward = new Vector3(1f, -0.6f, 0f).normalized;

            _prank.Tick(0.1f);

            Assert.AreEqual(2f, _prank.FloorPoint.z, 1e-4f);
        }

        [Test]
        public void Move_Placed_SlidesAwayFromTheCamera()
        {
            PlaceAhead();

            _prank.Move(new Vector2(0f, 0.1f));

            Assert.AreEqual(2.4f, _prank.FloorPoint.z, 1e-4f);
        }

        [Test]
        public void Move_Aiming_Ignored()
        {
            _prank.Begin();
            _prank.Tick(0.1f);

            _prank.Move(new Vector2(0.3f, 0.3f));

            Assert.AreEqual(2f, _prank.FloorPoint.z, 1e-4f);
        }

        [Test]
        public void Move_TowardsTheCamera_StopsAtTheNearest()
        {
            PlaceAhead();

            _prank.Move(new Vector2(0f, -5f));

            Assert.AreEqual(MinDistance, _prank.FloorPoint.z, 1e-4f);
        }

        [Test]
        public void Turn_Placed_AddsToTheFacing()
        {
            PlaceAhead();

            _prank.Turn(30f);

            Assert.AreEqual(210f, Mathf.Repeat(_prank.Yaw, 360f), 1e-3f);
        }

        [Test]
        public void Resize_Placed_ScalesTheViewAndItsHover()
        {
            PlaceAhead();

            _prank.Resize(1.5f);

            Assert.AreEqual(1.5f, _spawner.Last.Scale, 1e-4f);
            Assert.AreEqual(Hover * 1.5f, _spawner.Last.Position.y, 1e-4f);
        }

        [Test]
        public void Resize_BeyondTheLimit_Clamped()
        {
            PlaceAhead();

            _prank.Resize(10f);

            Assert.AreEqual(1.6f, _prank.Scale, 1e-4f);
        }

        [Test]
        public void Select_AnotherGhost_ReplacesTheViewAndStartsOver()
        {
            PlaceAhead();
            _prank.Resize(1.5f);

            _prank.Select(_banshee);

            Assert.IsTrue(_spawner.Spawned[0].IsDespawned);
            Assert.AreEqual(_banshee, _prank.Ghost.Value);
            Assert.AreEqual(PrankPhase.Aiming, _prank.Phase.Value);
            Assert.AreEqual(1f, _prank.Scale);
        }

        [Test]
        public void Select_GhostNotCaught_Ignored()
        {
            _prank.Begin();

            _prank.Select(_shade);

            Assert.AreEqual(_wisp, _prank.Ghost.Value);
        }

        [Test]
        public void Lift_Placed_FollowsTheCameraAgain()
        {
            PlaceAhead();

            _prank.Lift();

            Assert.AreEqual(PrankPhase.Aiming, _prank.Phase.Value);
        }

        [Test]
        public void ShootAsync_Posed_CapturesACaseFilePhotoWithoutStars()
        {
            PlaceAhead();

            var record = _prank.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("wisp", record.GhostId);
            Assert.AreEqual(0, record.Stars);
            Assert.AreEqual(_wisp, _capture.LastCaption.Ghost);
            Assert.AreEqual(0, _capture.LastCaption.Stars);
        }

        [Test]
        public void ShootAsync_DuringTheShot_GhostLooksAsPosed()
        {
            PlaceAhead();
            var litDuringShot = true;
            _capture.OnCapture = () => litDuringShot = _spawner.Last.IsFlashLit;

            _prank.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(litDuringShot);
            Assert.AreEqual(1f, _spawner.Last.Reveal);
        }

        [Test]
        public void ShootAsync_Posed_ReleasesTheShutter()
        {
            PlaceAhead();
            var released = 0;
            _prank.ShutterReleased += () => released++;

            _prank.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, released);
        }

        [Test]
        public void ShootAsync_SecondShot_DropsTheFirstFile()
        {
            PlaceAhead();
            var first = _prank.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            _prank.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            CollectionAssert.Contains(_storage.Deleted, first.FileName);
        }

        [Test]
        public void ShootAsync_NoGhostCaught_TakesNothing()
        {
            var prank = new PrankPhotoMode(new PlayerProgress(), new GhostConfig(new[] { _wisp }, "wisp", 1f, 1f, 1f), _camera,
                new FakePlaneProvider(), _spawner, _capture, _storage, new PrankPhotoConfig(), new FakeClock());
            prank.Begin();

            var record = prank.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsNull(record);
        }

        [Test]
        public void End_AfterAShot_ClearsTheGhostAndTheFile()
        {
            PlaceAhead();
            var record = _prank.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            _prank.End();

            Assert.IsTrue(_spawner.Last.IsDespawned);
            Assert.AreEqual(PrankPhase.Off, _prank.Phase.Value);
            CollectionAssert.Contains(_storage.Deleted, record.FileName);
        }

        [Test]
        public void Dispose_AfterAShot_DropsTheFile()
        {
            PlaceAhead();
            var record = _prank.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            _prank.Dispose();

            CollectionAssert.Contains(_storage.Deleted, record.FileName);
        }

        private void PlaceAhead()
        {
            _prank.Begin();
            _prank.Tick(0.1f);
            _prank.Place();
        }

        private static GhostData Ghost(string id)
        {
            var ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(ghost);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_motion._hoverHeightMin").floatValue = Hover;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return ghost;
        }
    }
}
