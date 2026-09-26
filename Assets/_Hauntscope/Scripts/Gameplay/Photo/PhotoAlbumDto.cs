using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Photo
{
    [Serializable]
    public sealed class PhotoAlbumDto
    {
        [SerializeField] private int _version;
        [SerializeField] private List<PhotoRecordDto> _photos = new List<PhotoRecordDto>();

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public PhotoAlbumDto()
        {
        }

        public PhotoAlbumDto(int version, List<PhotoRecordDto> photos)
        {
            _version = version;
            _photos = photos;
        }

        public int Version => _version;

        public IReadOnlyList<PhotoRecordDto> Photos => _photos;
    }
}
