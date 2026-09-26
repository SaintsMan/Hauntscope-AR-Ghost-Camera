using Hauntscope.Gameplay.Photo;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PhotoAlbumTests
    {
        private const int Limit = 3;

        private FakePhotoStorage _storage;
        private PhotoAlbum _album;

        [SetUp]
        public void SetUp()
        {
            _storage = new FakePhotoStorage();
            _album = new PhotoAlbum(_storage, TestConfigs.Photo(albumLimit: Limit));
        }

        [Test]
        public void Add_UnderTheLimit_KeepsEveryPhoto()
        {
            _album.Add(Photo("a", "wisp", 1));
            _album.Add(Photo("b", "wisp", 2));

            Assert.AreEqual(2, _album.Photos.Count);
        }

        [Test]
        public void Add_Always_RaisesChanged()
        {
            var raised = 0;
            _album.Changed += () => raised++;

            _album.Add(Photo("a", "wisp", 1));

            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Add_OverTheLimit_DropsTheOldestAndDeletesItsFile()
        {
            _album.Add(Photo("a", "wisp", 1));
            _album.Add(Photo("b", "wisp", 3));
            _album.Add(Photo("c", "shade", 2));

            _album.Add(Photo("d", "shade", 1));

            Assert.AreEqual(Limit, _album.Photos.Count);
            Assert.AreEqual("b", _album.Photos[0].FileName);
            CollectionAssert.AreEqual(new[] { "a" }, _storage.Deleted);
        }

        [Test]
        public void Add_OverTheLimit_NeverDropsAGhostsBestShot()
        {
            _album.Add(Photo("a", "wisp", 3));
            _album.Add(Photo("b", "shade", 3));
            _album.Add(Photo("c", "wisp", 1));

            _album.Add(Photo("d", "wisp", 2));

            Assert.AreEqual("a", _album.BestFor("wisp").FileName);
            CollectionAssert.AreEqual(new[] { "c" }, _storage.Deleted);
        }

        [Test]
        public void BestFor_SeveralShots_PicksTheMostStars()
        {
            _album.Add(Photo("a", "wisp", 1));
            _album.Add(Photo("b", "wisp", 3));
            _album.Add(Photo("c", "wisp", 2));

            Assert.AreEqual("b", _album.BestFor("wisp").FileName);
        }

        [Test]
        public void BestFor_NoShots_IsNull()
        {
            Assert.IsNull(_album.BestFor("wisp"));
        }

        [Test]
        public void CountFor_MixedGhosts_CountsOnlyThatGhost()
        {
            _album.Add(Photo("a", "wisp", 1));
            _album.Add(Photo("b", "shade", 1));
            _album.Add(Photo("c", "wisp", 1));

            Assert.AreEqual(2, _album.CountFor("wisp"));
        }

        private static PhotoRecord Photo(string file, string ghost, int stars)
        {
            return new PhotoRecord(file, ghost, stars, 0L);
        }
    }
}
