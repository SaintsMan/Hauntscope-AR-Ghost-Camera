using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class RoomConfig
    {
        [SerializeField, Min(0.01f)] private float _calibrationArea = 1.5f;

        public RoomConfig()
        {
        }

        public RoomConfig(float calibrationArea)
        {
            _calibrationArea = calibrationArea;
        }

        public float CalibrationArea => _calibrationArea;
    }
}
