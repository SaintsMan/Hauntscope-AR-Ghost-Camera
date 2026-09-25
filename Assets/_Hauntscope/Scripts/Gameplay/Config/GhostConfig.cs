using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostConfig
    {
        [SerializeField] private GhostData[] _ghosts = Array.Empty<GhostData>();
        [SerializeField, Min(0f)] private float _wanderIntervalMin = 3f;
        [SerializeField, Min(0f)] private float _wanderIntervalMax = 6f;
        [SerializeField, Min(0.01f)] private float _moveSmoothTime = 0.8f;
        [SerializeField, Min(0f)] private float _bobAmplitude = 0.05f;
        [SerializeField, Min(0f)] private float _bobFrequency = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _alertRevealThreshold = 0.5f;
        [SerializeField, Min(0f)] private float _alertedPauseDuration = 0.5f;
        [SerializeField, Min(1f)] private float _alertedSpeedMultiplier = 1.5f;

        public GhostConfig()
        {
        }

        public GhostConfig(
            float wanderIntervalMin,
            float wanderIntervalMax,
            float moveSmoothTime,
            float bobAmplitude,
            float bobFrequency,
            float alertRevealThreshold,
            float alertedPauseDuration,
            float alertedSpeedMultiplier)
        {
            _wanderIntervalMin = wanderIntervalMin;
            _wanderIntervalMax = wanderIntervalMax;
            _moveSmoothTime = moveSmoothTime;
            _bobAmplitude = bobAmplitude;
            _bobFrequency = bobFrequency;
            _alertRevealThreshold = alertRevealThreshold;
            _alertedPauseDuration = alertedPauseDuration;
            _alertedSpeedMultiplier = alertedSpeedMultiplier;
        }

        public IReadOnlyList<GhostData> Ghosts => _ghosts;

        public float WanderIntervalMin => _wanderIntervalMin;

        public float WanderIntervalMax => _wanderIntervalMax;

        public float MoveSmoothTime => _moveSmoothTime;

        public float BobAmplitude => _bobAmplitude;

        public float BobFrequency => _bobFrequency;

        public float AlertRevealThreshold => _alertRevealThreshold;

        public float AlertedPauseDuration => _alertedPauseDuration;

        public float AlertedSpeedMultiplier => _alertedSpeedMultiplier;
    }
}
