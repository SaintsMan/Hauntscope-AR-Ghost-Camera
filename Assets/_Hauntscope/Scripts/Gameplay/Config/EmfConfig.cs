using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class EmfConfig
    {
        [SerializeField, Min(1)] private int _maxLevel = 5;
        [SerializeField, Range(0f, 1f)] private float _facingMinFactor = 0.6f;
        [SerializeField, Min(0f)] private float _noiseAmplitude = 0.05f;
        [SerializeField, Min(0f)] private float _noiseInterval = 0.2f;
        [SerializeField, Min(0.01f)] private float _beepIntervalSlow = 1.2f;
        [SerializeField, Min(0.01f)] private float _beepIntervalFast = 0.1f;
        [SerializeField] private AudioClip _beepClip;
        [SerializeField, Range(0f, 1f)] private float _beepVolume = 0.6f;
        [SerializeField, Min(0.1f)] private float _beepPitchMin = 1f;
        [SerializeField, Min(0.1f)] private float _beepPitchMax = 1.6f;
        [SerializeField, Min(1)] private int _hapticLevel = 4;

        public EmfConfig()
        {
        }

        public EmfConfig(
            int maxLevel,
            float facingMinFactor,
            float noiseAmplitude,
            float noiseInterval,
            float beepIntervalSlow,
            float beepIntervalFast,
            float beepPitchMin,
            float beepPitchMax,
            int hapticLevel)
        {
            _maxLevel = maxLevel;
            _facingMinFactor = facingMinFactor;
            _noiseAmplitude = noiseAmplitude;
            _noiseInterval = noiseInterval;
            _beepIntervalSlow = beepIntervalSlow;
            _beepIntervalFast = beepIntervalFast;
            _beepPitchMin = beepPitchMin;
            _beepPitchMax = beepPitchMax;
            _hapticLevel = hapticLevel;
        }

        public int MaxLevel => _maxLevel;

        public float FacingMinFactor => _facingMinFactor;

        public float NoiseAmplitude => _noiseAmplitude;

        public float NoiseInterval => _noiseInterval;

        public float BeepIntervalSlow => _beepIntervalSlow;

        public float BeepIntervalFast => _beepIntervalFast;

        public AudioClip BeepClip => _beepClip;

        public float BeepVolume => _beepVolume;

        public float BeepPitchMin => _beepPitchMin;

        public float BeepPitchMax => _beepPitchMax;

        public int HapticLevel => _hapticLevel;
    }
}
