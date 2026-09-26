using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Photo
{
    // Every photo the agent took, newest last. When full, the oldest photo goes, but never the best shot of a ghost:
    // that one is its evidence in the Bestiary.
    public sealed class PhotoAlbum
    {
        private readonly List<PhotoRecord> _photos = new List<PhotoRecord>();
        private readonly IPhotoStorage _storage;
        private readonly PhotoConfig _config;

        public PhotoAlbum(IPhotoStorage storage, PhotoConfig config)
        {
            _storage = storage;
            _config = config;
        }

        public event Action Changed;

        public IReadOnlyList<PhotoRecord> Photos => _photos;

        public void Restore(IEnumerable<PhotoRecord> photos)
        {
            _photos.Clear();
            _photos.AddRange(photos);
        }

        public void Add(PhotoRecord photo)
        {
            _photos.Add(photo);
            while (_photos.Count > _config.AlbumLimit && TryEvictOldest())
            {
            }

            Changed?.Invoke();
        }

        public PhotoRecord BestFor(string ghostId)
        {
            PhotoRecord best = null;
            foreach (var photo in _photos)
            {
                if (photo.GhostId == ghostId && (best == null || photo.Stars > best.Stars))
                    best = photo;
            }

            return best;
        }

        public int CountFor(string ghostId)
        {
            var count = 0;
            foreach (var photo in _photos)
            {
                if (photo.GhostId == ghostId)
                    count++;
            }

            return count;
        }

        private bool TryEvictOldest()
        {
            for (var i = 0; i < _photos.Count; i++)
            {
                var photo = _photos[i];
                if (BestFor(photo.GhostId) == photo)
                    continue;

                _photos.RemoveAt(i);
                _storage.Delete(photo.FileName);
                return true;
            }

            return false;
        }
    }
}
