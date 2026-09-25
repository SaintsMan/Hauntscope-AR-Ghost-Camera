using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostConfig
    {
        [SerializeField] private GhostData[] _ghosts = Array.Empty<GhostData>();
        [SerializeField] private string _firstGhostId = "wisp";
        [SerializeField, Min(0f)] private float _commonWeight = 75f;
        [SerializeField, Min(0f)] private float _rareWeight = 25f;
        [SerializeField, Min(0f)] private float _legendaryWeight;
        [SerializeField, Min(0f)] private float _wanderIntervalMin = 3f;
        [SerializeField, Min(0f)] private float _wanderIntervalMax = 6f;
        [SerializeField, Min(0.01f)] private float _moveSmoothTime = 0.8f;
        [SerializeField, Min(0f)] private float _bobAmplitude = 0.05f;
        [SerializeField, Min(0f)] private float _bobFrequency = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _alertRevealThreshold = 0.5f;
        [SerializeField, Min(0f)] private float _alertedPauseDuration = 0.5f;
        [SerializeField, Min(1f)] private float _alertedSpeedMultiplier = 1.5f;
        [SerializeField, Min(0f)] private float _fleeCalmDownTime = 2f;
        [SerializeField, Min(0.1f)] private float _fleeDistance = 2f;
        [SerializeField, Min(0.05f)] private float _fleeRetargetInterval = 0.6f;
        [SerializeField, Min(0.1f)] private float _captureDuration = 1.2f;
        [SerializeField, Min(0f)] private float _captureSpiralTurns = 2f;
        [SerializeField, Min(0f)] private float _captureSpiralRadius = 0.3f;
        [SerializeField, Min(0.1f)] private float _escapeDuration = 1f;

        public GhostConfig()
        {
        }

        public GhostConfig(
            GhostData[] ghosts,
            string firstGhostId,
            float commonWeight,
            float rareWeight,
            float legendaryWeight)
        {
            _ghosts = ghosts;
            _firstGhostId = firstGhostId;
            _commonWeight = commonWeight;
            _rareWeight = rareWeight;
            _legendaryWeight = legendaryWeight;
        }

        public GhostConfig(
            float wanderIntervalMin,
            float wanderIntervalMax,
            float moveSmoothTime,
            float bobAmplitude,
            float bobFrequency,
            float alertRevealThreshold,
            float alertedPauseDuration,
            float alertedSpeedMultiplier,
            float fleeCalmDownTime,
            float fleeDistance,
            float fleeRetargetInterval,
            float captureDuration,
            float captureSpiralTurns,
            float captureSpiralRadius,
            float escapeDuration)
        {
            _wanderIntervalMin = wanderIntervalMin;
            _wanderIntervalMax = wanderIntervalMax;
            _moveSmoothTime = moveSmoothTime;
            _bobAmplitude = bobAmplitude;
            _bobFrequency = bobFrequency;
            _alertRevealThreshold = alertRevealThreshold;
            _alertedPauseDuration = alertedPauseDuration;
            _alertedSpeedMultiplier = alertedSpeedMultiplier;
            _fleeCalmDownTime = fleeCalmDownTime;
            _fleeDistance = fleeDistance;
            _fleeRetargetInterval = fleeRetargetInterval;
            _captureDuration = captureDuration;
            _captureSpiralTurns = captureSpiralTurns;
            _captureSpiralRadius = captureSpiralRadius;
            _escapeDuration = escapeDuration;
        }

        public IReadOnlyList<GhostData> Ghosts => _ghosts;

        public string FirstGhostId => _firstGhostId;

        public float GetRarityWeight(GhostRarity rarity)
        {
            return rarity switch
            {
                GhostRarity.Common => _commonWeight,
                GhostRarity.Rare => _rareWeight,
                _ => _legendaryWeight
            };
        }

        public float WanderIntervalMin => _wanderIntervalMin;

        public float WanderIntervalMax => _wanderIntervalMax;

        public float MoveSmoothTime => _moveSmoothTime;

        public float BobAmplitude => _bobAmplitude;

        public float BobFrequency => _bobFrequency;

        public float AlertRevealThreshold => _alertRevealThreshold;

        public float AlertedPauseDuration => _alertedPauseDuration;

        public float AlertedSpeedMultiplier => _alertedSpeedMultiplier;

        public float FleeCalmDownTime => _fleeCalmDownTime;

        public float FleeDistance => _fleeDistance;

        public float FleeRetargetInterval => _fleeRetargetInterval;

        public float CaptureDuration => _captureDuration;

        public float CaptureSpiralTurns => _captureSpiralTurns;

        public float CaptureSpiralRadius => _captureSpiralRadius;

        public float EscapeDuration => _escapeDuration;
    }
}
