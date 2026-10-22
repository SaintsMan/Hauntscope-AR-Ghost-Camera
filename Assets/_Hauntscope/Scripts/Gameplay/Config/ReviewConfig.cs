using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class ReviewConfig
    {
        [SerializeField, Min(1)] private int _minCaptures = 1;
        [SerializeField, Range(0, 3)] private int _minPhotoStars = 3;
        [SerializeField, Min(1)] private int _repeatEveryCaptures = 20;
        [SerializeField, Min(0f)] private float _menuDelay = 1.5f;

        public ReviewConfig()
        {
        }

        public ReviewConfig(int minCaptures, int repeatEveryCaptures, float menuDelay, int minPhotoStars = 0)
        {
            _minPhotoStars = minPhotoStars;
            _minCaptures = minCaptures;
            _repeatEveryCaptures = repeatEveryCaptures;
            _menuDelay = menuDelay;
        }

        // Catches before the first ask (Remote Config review_min_captures).
        public int MinCaptures => _minCaptures;

        // A kept photo this good asks too, catch or not; 0 = photos never ask (review_min_photo_stars).
        public int MinPhotoStars => _minPhotoStars;

        public int RepeatEveryCaptures => _repeatEveryCaptures;

        public float MenuDelay => _menuDelay;
    }
}
