using Hauntscope.Gameplay.Photo;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PhotoAlbumRepositoryTests
    {
        private FakeSaveService _save;
        private FakePhotoStorage _storage;
        private PhotoAlbumRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _save = new FakeSaveService();
            _storage = new FakePhotoStorage();
            _repository = new PhotoAlbumRepository(_save, _storage);
        }

        [Test]
        public void Load_NothingSaved_IsEmpty()
        {
            Assert.AreEqual(0, _repository.Load().Count);
        }

        [Test]
        public void SaveThenLoad_Photos_RoundTrip()
        {
            _storage.Put("a.jpg");
            var album = new PhotoAlbum(_storage, TestConfigs.Photo());
            album.Add(new PhotoRecord("a.jpg", "wisp", 2, 1790000000L));

            _repository.Save(album);
            var loaded = _repository.Load();

            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual("a.jpg", loaded[0].FileName);
            Assert.AreEqual("wisp", loaded[0].GhostId);
            Assert.AreEqual(2, loaded[0].Stars);
            Assert.AreEqual(1790000000L, loaded[0].TakenUnixSeconds);
        }

        [Test]
        public void Load_FileMissing_DropsThatRecord()
        {
            _storage.Put("a.jpg");
            var album = new PhotoAlbum(_storage, TestConfigs.Photo());
            album.Add(new PhotoRecord("a.jpg", "wisp", 2, 0L));
            album.Add(new PhotoRecord("gone.jpg", "wisp", 3, 0L));
            _repository.Save(album);

            var loaded = _repository.Load();

            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual("a.jpg", loaded[0].FileName);
        }

        [Test]
        public void Load_NewerVersion_StartsEmpty()
        {
            _save.SetRaw("photo_album", "{\"_version\":99,\"_photos\":[]}");

            Assert.AreEqual(0, _repository.Load().Count);
        }
    }
}
