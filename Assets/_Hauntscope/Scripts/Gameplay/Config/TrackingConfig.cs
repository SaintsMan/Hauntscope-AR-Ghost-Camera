using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class TrackingConfig
    {
        [SerializeField, Min(0f)] private float _lostGrace = 0.5f;

        public TrackingConfig()
        {
        }

        public TrackingConfig(float lostGrace)
        {
            _lostGrace = lostGrace;
        }

        public float LostGrace => _lostGrace;
    }
}
