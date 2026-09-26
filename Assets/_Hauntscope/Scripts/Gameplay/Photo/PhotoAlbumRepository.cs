using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Photo
{
    public sealed class PhotoAlbumRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "photo_album";

        private readonly ISaveService _save;
        private readonly IPhotoStorage _storage;

        public PhotoAlbumRepository(ISaveService save, IPhotoStorage storage)
        {
            _save = save;
            _storage = storage;
        }

        // Records whose file is gone (cleared app storage, a failed write) are dropped instead of showing a hole.
        public IReadOnlyList<PhotoRecord> Load()
        {
            var photos = new List<PhotoRecord>();
            if (!_save.TryLoad<PhotoAlbumDto>(Key, out var dto) || dto.Version < 1 || dto.Version > CurrentVersion)
                return photos;

            foreach (var entry in dto.Photos)
            {
                if (string.IsNullOrEmpty(entry.FileName) || string.IsNullOrEmpty(entry.GhostId) || !_storage.Exists(entry.FileName))
                    continue;

                photos.Add(new PhotoRecord(entry.FileName, entry.GhostId, entry.Stars, entry.TakenUnixSeconds));
            }

            return photos;
        }

        public void Save(PhotoAlbum album)
        {
            var photos = new List<PhotoRecordDto>(album.Photos.Count);
            foreach (var photo in album.Photos)
                photos.Add(new PhotoRecordDto(photo.FileName, photo.GhostId, photo.Stars, photo.TakenUnixSeconds));

            _save.Save(Key, new PhotoAlbumDto(CurrentVersion, photos));
        }
    }
}
