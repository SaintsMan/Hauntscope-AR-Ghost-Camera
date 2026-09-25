using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class ReviewConfig
    {
        [SerializeField, Min(1)] private int _minCaptures = 3;
        [SerializeField, Min(1)] private int _repeatEveryCaptures = 20;
        [SerializeField, Min(0f)] private float _menuDelay = 1.5f;

        public ReviewConfig()
        {
        }

        public ReviewConfig(int minCaptures, int repeatEveryCaptures, float menuDelay)
        {
            _minCaptures = minCaptures;
            _repeatEveryCaptures = repeatEveryCaptures;
            _menuDelay = menuDelay;
        }

        public int MinCaptures => _minCaptures;

        public int RepeatEveryCaptures => _repeatEveryCaptures;

        public float MenuDelay => _menuDelay;
    }
}
