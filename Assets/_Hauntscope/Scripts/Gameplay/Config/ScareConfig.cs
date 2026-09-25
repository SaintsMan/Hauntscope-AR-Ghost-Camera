using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class ScareConfig
    {
        [SerializeField, Min(0f)] private float _minTime = 30f;
        [SerializeField, Min(0f)] private float _distance = 1.5f;
        [SerializeField, Range(1f, 180f)] private float _angle = 45f;
        [SerializeField, Min(0.1f)] private float _duration = 1.2f;
        [SerializeField, Min(0.01f)] private float _rushTime = 0.3f;
        [SerializeField, Min(0.1f)] private float _faceDistance = 0.45f;

        public ScareConfig()
        {
        }

        public ScareConfig(float minTime, float distance, float angle, float duration, float rushTime, float faceDistance)
        {
            _minTime = minTime;
            _distance = distance;
            _angle = angle;
            _duration = duration;
            _rushTime = rushTime;
            _faceDistance = faceDistance;
        }

        public float MinTime => _minTime;

        public float Distance => _distance;

        public float Angle => _angle;

        public float Duration => _duration;

        public float RushTime => _rushTime;

        public float FaceDistance => _faceDistance;
    }
}
