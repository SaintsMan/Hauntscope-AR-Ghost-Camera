using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SpiritCameraTests
    {
        private const int Film = 3;
        private const float Cooldown = 1f;

        private GhostFixture _fixture;
        private GhostData _data;
        private FakePhotoStorage _storage;
        private FakePhotoCapture _capture;
        private PhotoAlbum _album;
        private FakeSaveService _save;
        private SpiritCamera _camera;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1.5f, 0f));
            _fixture.Camera.Position = new Vector3(0f, 1.5f, -3f);
            _fixture.Ghost.Start();
            _data = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(_data);
            serialized.FindProperty("_id").stringValue = "wisp";
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var session = new HuntSession();
            session.Begin(_fixture.Ghost, _data);
            var config = TestConfigs.Photo(filmPerHunt: Film, shutterCooldown: Cooldown);
            _storage = new FakePhotoStorage();
            _capture = new FakePhotoCapture(_storage);
            _album = new PhotoAlbum(_storage, config);
            _save = new FakeSaveService();
            _camera = new SpiritCamera(session, new PhotoScorer(_fixture.Camera, config), _capture, _album,
                new PhotoAlbumRepository(_save, _storage), config, new FakeClock());
            _camera.BeginHunt();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void BeginHunt_Always_LoadsFreshFilm()
        {
            Assert.AreEqual(Film, _camera.Film.Value);
        }

        [Test]
        public void Tick_GhostInFrame_ArmsTheShutter()
        {
            _fixture.Ghost.SetReveal(1f);

            _camera.Tick(0.1f);

            Assert.IsTrue(_camera.CanShoot.Value);
        }

        [Test]
        public void Tick_GhostHidden_KeepsTheShutterDisarmed()
        {
            _camera.Tick(0.1f);

            Assert.IsFalse(_camera.CanShoot.Value);
        }

        [Test]
        public void ShootAsync_Armed_SpendsFilmStoresAndSavesThePhoto()
        {
            Arm();

            var taken = Shoot();

            Assert.IsTrue(taken);
            Assert.AreEqual(Film - 1, _camera.Film.Value);
            Assert.AreEqual(1, _album.Photos.Count);
            Assert.AreEqual("wisp", _album.Photos[0].GhostId);
            Assert.AreEqual(1, _save.SaveCount);
        }

        [Test]
        public void ShootAsync_NotArmed_DoesNothing()
        {
            var taken = _camera.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(taken);
            Assert.AreEqual(Film, _camera.Film.Value);
        }

        [Test]
        public void ShootAsync_Armed_RaisesShutterThenPhotoTaken()
        {
            var shutter = 0;
            PhotoShot shot = null;
            _camera.ShutterReleased += () => shutter++;
            _camera.PhotoTaken += taken => shot = taken;
            Arm();

            Shoot();

            Assert.AreEqual(1, shutter);
            Assert.IsNotNull(shot);
        }

        [Test]
        public void ShootAsync_CalmGhost_FlashSpooksItIntoFleeing()
        {
            Arm();
            _fixture.Ghost.Tick(0.01f);

            Shoot();
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsFleeing);
        }

        [Test]
        public void ShootAsync_CaptionCarriesTheScore()
        {
            _fixture.Camera.Projection = point => new Vector3(0.5f, 0.5f + (point.y - 1.5f) * 0.4f, 1f);
            Arm();

            Shoot();

            Assert.AreEqual(2, _capture.LastCaption.Stars);
            Assert.AreSame(_data, _capture.LastCaption.Ghost);
        }

        [Test]
        public void Tick_RightAfterAShot_WaitsForTheCooldown()
        {
            Arm();
            Shoot();

            _camera.Tick(Cooldown * 0.5f);

            Assert.IsFalse(_camera.CanShoot.Value);
        }

        [Test]
        public void Tick_OutOfFilm_StaysDisarmed()
        {
            for (var i = 0; i < Film; i++)
            {
                _camera.Tick(Cooldown);
                Shoot();
            }

            _camera.Tick(Cooldown);

            Assert.AreEqual(0, _camera.Film.Value);
            Assert.IsFalse(_camera.CanShoot.Value);
        }

        [Test]
        public void ShootAsync_SeveralShots_KeepsTheBestAndCountsEvidence()
        {
            Arm();
            Shoot();
            _fixture.Camera.Projection = point => new Vector3(0.5f, 0.5f + (point.y - 1.5f) * 0.4f, 1f);
            _camera.Tick(Cooldown);

            Shoot();

            Assert.AreEqual(2, _camera.BestShot.Score.Stars);
            Assert.AreEqual(1, _camera.EvidenceShots);
            Assert.AreEqual(TestConfigs.Photo().RewardFor(2), _camera.Reward);
        }

        [Test]
        public void ShootAsync_CaptureFails_GivesTheFilmBack()
        {
            _capture.Fails = true;
            Arm();

            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("photo failed"));
            var taken = Shoot();

            Assert.IsFalse(taken);
            Assert.AreEqual(Film, _camera.Film.Value);
        }

        [Test]
        public void BeginHunt_ExtraFilm_LoadsItOnTop()
        {
            _camera.BeginHunt(2);

            Assert.AreEqual(Film + 2, _camera.Film.Value);
        }

        [Test]
        public void BeginHunt_AfterShots_ForgetsThePreviousHunt()
        {
            Arm();
            Shoot();

            _camera.BeginHunt();

            Assert.IsNull(_camera.BestShot);
            Assert.AreEqual(0, _camera.EvidenceShots);
            Assert.AreEqual(Film, _camera.Film.Value);
        }

        [Test]
        public void ShootAsync_Armed_FlashLightsTheGhostOnlyForThePicture()
        {
            var litDuringCapture = false;
            _capture.OnCapture = () => litDuringCapture = _fixture.View.IsFlashLit;
            Arm();

            Shoot();

            Assert.IsTrue(litDuringCapture);
            Assert.IsFalse(_fixture.View.IsFlashLit);
        }

        private void Arm()
        {
            _fixture.Ghost.SetReveal(1f);
            _camera.Tick(0.01f);
        }

        private bool Shoot()
        {
            _fixture.Ghost.SetReveal(1f);
            _camera.Tick(0f);
            return _camera.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
