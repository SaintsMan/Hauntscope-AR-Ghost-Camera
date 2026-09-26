using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class RoomConfig
    {
        [SerializeField, Min(0.01f)] private float _calibrationArea = 1.5f;
        [SerializeField, Min(0f)] private float _calibratedMessageDuration = 1.5f;
        [SerializeField, Min(0f)] private float _roomBoundsPadding = 0.3f;
        [SerializeField, Min(0f)] private float _spawnMinDistance = 2f;
        [SerializeField, Min(0f)] private float _spawnMaxDistance = 5f;
        [SerializeField, Min(0f)] private float _floorTolerance = 0.15f;

        public RoomConfig()
        {
        }

        public RoomConfig(
            float calibrationArea,
            float calibratedMessageDuration,
            float roomBoundsPadding,
            float spawnMinDistance,
            float spawnMaxDistance)
        {
            _calibrationArea = calibrationArea;
            _calibratedMessageDuration = calibratedMessageDuration;
            _roomBoundsPadding = roomBoundsPadding;
            _spawnMinDistance = spawnMinDistance;
            _spawnMaxDistance = spawnMaxDistance;
        }

        public float CalibrationArea => _calibrationArea;

        public float CalibratedMessageDuration => _calibratedMessageDuration;

        public float RoomBoundsPadding => _roomBoundsPadding;

        // AR: a horizontal plane counts as floor when it is this close to the lowest one, so tables are not floor.
        public float FloorTolerance => _floorTolerance;

        public float SpawnMinDistance => _spawnMinDistance;

        public float SpawnMaxDistance => _spawnMaxDistance;
    }
}
