using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class EmergencyConfig
    {
        [SerializeField, Min(1f)] private float _countdown = 8f;
        [SerializeField, Range(0f, 1f)] private float _adCharge = 0.5f;

        public EmergencyConfig()
        {
        }

        public EmergencyConfig(float countdown, float adCharge)
        {
            _countdown = countdown;
            _adCharge = adCharge;
        }

        // Seconds the "battery dead" card waits before the ghost gets away.
        public float Countdown => _countdown;

        // Fraction of the battery a rewarded ad restores.
        public float AdCharge => _adCharge;
    }
}
