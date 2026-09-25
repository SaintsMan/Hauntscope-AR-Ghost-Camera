using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class HapticsConfig
    {
        [SerializeField, Min(1)] private int _lightDurationMs = 20;
        [SerializeField, Range(1, 255)] private int _lightAmplitude = 80;
        [SerializeField, Min(1)] private int _mediumDurationMs = 35;
        [SerializeField, Range(1, 255)] private int _mediumAmplitude = 160;
        [SerializeField, Min(1)] private int _heavyDurationMs = 60;
        [SerializeField, Range(1, 255)] private int _heavyAmplitude = 255;

        public int LightDurationMs => _lightDurationMs;

        public int LightAmplitude => _lightAmplitude;

        public int MediumDurationMs => _mediumDurationMs;

        public int MediumAmplitude => _mediumAmplitude;

        public int HeavyDurationMs => _heavyDurationMs;

        public int HeavyAmplitude => _heavyAmplitude;
    }
}
