using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostCapture
    {
        [SerializeField, Min(0.01f)] private float _resistance = 1f;

        public GhostCapture()
        {
        }

        public GhostCapture(float resistance)
        {
            _resistance = resistance;
        }

        public float Resistance => _resistance;
    }
}
