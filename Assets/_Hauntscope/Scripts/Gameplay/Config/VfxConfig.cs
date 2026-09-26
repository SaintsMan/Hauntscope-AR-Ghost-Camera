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
        [SerializeField] private ParticleSystem _pickupBurst;
        [SerializeField] private ParticleSystem _staggerSparks;
        [SerializeField] private ParticleSystem _coldSpot;
        [SerializeField] private ParticleSystem _dashStreak;
        [SerializeField] private ParticleSystem _shriekWave;
        [SerializeField] private Color _coldSpotColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField, Min(0f)] private float _beamOriginForward = 0.3f;
        [SerializeField, Min(0f)] private float _beamOriginDrop = 0.2f;
        [SerializeField, Min(0.5f)] private float _beamRange = 4f;
        [SerializeField, Range(0f, 1f)] private float _beamIdleIntensity = 0.45f;
        [SerializeField, Range(0f, 1f)] private float _beamLockedIntensity = 0.75f;
        [SerializeField, Range(0f, 1f)] private float _revealPulseThreshold = 0.6f;

        public ParticleSystem CaptureSpiral => _captureSpiral;

        public ParticleSystem TeleportFlash => _teleportFlash;

        public ParticleSystem RevealPulse => _revealPulse;

        public ParticleSystem PickupBurst => _pickupBurst;

        public ParticleSystem StaggerSparks => _staggerSparks;

        public ParticleSystem ColdSpot => _coldSpot;

        // Where the wraith dashed from: air torn in streaks and a puff of its smoke.
        public ParticleSystem DashStreak => _dashStreak;

        // The banshee's cry as rings rolling out from her.
        public ParticleSystem ShriekWave => _shriekWave;

        // Frost is not the ghost's own glow: it stays the same icy white whoever is hiding, so it reads as a clue.
        public Color ColdSpotColor => _coldSpotColor;

        // The beam leaves from just below and in front of the lens, so it rises into view from the bottom edge.
        public float BeamOriginForward => _beamOriginForward;

        public float BeamOriginDrop => _beamOriginDrop;

        public float BeamRange => _beamRange;

        public float BeamIdleIntensity => _beamIdleIntensity;

        public float BeamLockedIntensity => _beamLockedIntensity;

        public float RevealPulseThreshold => _revealPulseThreshold;
    }
}
