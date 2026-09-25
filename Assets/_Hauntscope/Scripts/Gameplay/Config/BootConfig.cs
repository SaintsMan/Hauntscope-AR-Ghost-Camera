using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class BootConfig
    {
        [SerializeField, Min(0f)] private float _introDuration = 2.2f;
        [SerializeField, Min(0f)] private float _stepMinDuration = 1f;
        [SerializeField, Min(0.1f)] private float _stepTimeout = 5f;
        [SerializeField, Min(0f)] private float _outroDuration = 0.8f;

        public BootConfig()
        {
        }

        public BootConfig(float introDuration, float stepMinDuration, float stepTimeout, float outroDuration)
        {
            _introDuration = introDuration;
            _stepMinDuration = stepMinDuration;
            _stepTimeout = stepTimeout;
            _outroDuration = outroDuration;
        }

        public float IntroDuration => _introDuration;

        public float StepMinDuration => _stepMinDuration;

        public float StepTimeout => _stepTimeout;

        public float OutroDuration => _outroDuration;
    }
}
