using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class ToolsConfig
    {
        [SerializeField, Min(1f)] private float _batteryMax = 100f;
        [SerializeField, Min(0f)] private float _passiveDrain = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _lowBatteryThreshold = 0.2f;
        [SerializeField, Min(0f)] private float _lensDrain = 3f;
        [SerializeField, Min(0f)] private float _beamDrain = 4f;
        [SerializeField, Range(1f, 90f)] private float _revealAngle = 30f;
        [SerializeField, Min(0.01f)] private float _revealInTime = 0.4f;
        [SerializeField, Min(0.01f)] private float _revealOutTime = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _beamRevealThreshold = 0.5f;
        [SerializeField, Range(0.01f, 0.5f)] private float _reticleRadius = 0.14f;
        [SerializeField, Min(0f)] private float _captureRate = 0.16f;
        [SerializeField, Min(0f)] private float _decayRate = 0.15f;

        public ToolsConfig()
        {
        }

        public ToolsConfig(
            float batteryMax,
            float passiveDrain,
            float lowBatteryThreshold,
            float lensDrain,
            float beamDrain,
            float revealAngle,
            float revealInTime,
            float revealOutTime,
            float beamRevealThreshold,
            float reticleRadius,
            float captureRate,
            float decayRate)
        {
            _batteryMax = batteryMax;
            _passiveDrain = passiveDrain;
            _lowBatteryThreshold = lowBatteryThreshold;
            _lensDrain = lensDrain;
            _beamDrain = beamDrain;
            _revealAngle = revealAngle;
            _revealInTime = revealInTime;
            _revealOutTime = revealOutTime;
            _beamRevealThreshold = beamRevealThreshold;
            _reticleRadius = reticleRadius;
            _captureRate = captureRate;
            _decayRate = decayRate;
        }

        public float BatteryMax => _batteryMax;

        public float PassiveDrain => _passiveDrain;

        public float LowBatteryThreshold => _lowBatteryThreshold;

        public float LensDrain => _lensDrain;

        public float BeamDrain => _beamDrain;

        public float RevealAngle => _revealAngle;

        public float RevealInTime => _revealInTime;

        public float RevealOutTime => _revealOutTime;

        public float BeamRevealThreshold => _beamRevealThreshold;

        public float ReticleRadius => _reticleRadius;

        public float CaptureRate => _captureRate;

        public float DecayRate => _decayRate;
    }
}
