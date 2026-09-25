using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class HudConfig
    {
        [SerializeField, Min(0.05f)] private float _recBlinkInterval = 0.6f;
        [SerializeField, Min(0.05f)] private float _lowBatteryBlinkInterval = 0.3f;
        [SerializeField, Min(1f)] private float _grainFrameRate = 24f;
        [SerializeField] private AudioClip _lowBatteryClip;
        [SerializeField, Range(0f, 1f)] private float _lowBatteryVolume = 0.7f;

        public float RecBlinkInterval => _recBlinkInterval;

        public float LowBatteryBlinkInterval => _lowBatteryBlinkInterval;

        public float GrainFrameRate => _grainFrameRate;

        public AudioClip LowBatteryClip => _lowBatteryClip;

        public float LowBatteryVolume => _lowBatteryVolume;
    }
}
