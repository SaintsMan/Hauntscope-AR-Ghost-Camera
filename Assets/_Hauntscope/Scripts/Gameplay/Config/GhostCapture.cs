using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostCapture
    {
        [SerializeField, Min(0.01f)] private float _resistance = 1f;
        [SerializeField, Min(0)] private int _reward = 10;

        public GhostCapture()
        {
        }

        public GhostCapture(float resistance, int reward)
        {
            _resistance = resistance;
            _reward = reward;
        }

        public float Resistance => _resistance;

        public int Reward => _reward;
    }
}
