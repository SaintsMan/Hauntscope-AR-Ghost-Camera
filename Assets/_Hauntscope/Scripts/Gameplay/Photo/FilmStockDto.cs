using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Photo
{
    [Serializable]
    public sealed class FilmStockDto
    {
        [SerializeField] private int _version;
        [SerializeField] private int _frames;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public FilmStockDto()
        {
        }

        public FilmStockDto(int version, int frames)
        {
            _version = version;
            _frames = frames;
        }

        public int Version => _version;

        public int Frames => _frames;
    }
}
