using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class VfxConfig
    {
        [SerializeField] private ParticleSystem _captureSpiral;
        [SerializeField] private ParticleSystem _teleportFlash;
        [SerializeField] private ParticleSystem _revealPulse;
        [SerializeField, Min(0f)] private float _beamOriginForward = 0.3f;
        [SerializeField, Min(0f)] private float _beamOriginDrop = 0.2f;
        [SerializeField, Min(0.5f)] private float _beamRange = 4f;
        [SerializeField, Range(0f, 1f)] private float _beamIdleIntensity = 0.45f;
        [SerializeField, Range(0f, 1f)] private float _beamLockedIntensity = 0.75f;
        [SerializeField, Range(0f, 1f)] private float _revealPulseThreshold = 0.6f;

        public ParticleSystem CaptureSpiral => _captureSpiral;

        public ParticleSystem TeleportFlash => _teleportFlash;

        public ParticleSystem RevealPulse => _revealPulse;

        // The beam leaves from just below and in front of the lens, so it rises into view from the bottom edge.
        public float BeamOriginForward => _beamOriginForward;

        public float BeamOriginDrop => _beamOriginDrop;

        public float BeamRange => _beamRange;

        public float BeamIdleIntensity => _beamIdleIntensity;

        public float BeamLockedIntensity => _beamLockedIntensity;

        public float RevealPulseThreshold => _revealPulseThreshold;
    }
}
