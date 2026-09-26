using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Store
{
    // What one piece of equipment changes in a hunt. Multipliers are 1 when untouched, so sets combine by product.
    [Serializable]
    public sealed class HuntModifierSet
    {
        [SerializeField, Min(0f)] private float _captureRate = 1f;
        [SerializeField, Min(0f)] private float _reticleRadius = 1f;
        [SerializeField, Min(0f)] private float _decayRate = 1f;
        [SerializeField, Min(0f)] private float _beamDrain = 1f;
        [SerializeField, Min(0f)] private float _lensDrain = 1f;
        [SerializeField, Min(0f)] private float _emfRange = 1f;
        [SerializeField, Min(0f)] private float _ghostSpeed = 1f;
        [SerializeField, Min(0f)] private float _beamedGhostSpeed = 1f;
        [SerializeField] private bool _locksHiddenGhosts;
        [SerializeField] private bool _showsEmfDirection;

        public HuntModifierSet()
        {
        }

        public HuntModifierSet(
            float captureRate = 1f,
            float reticleRadius = 1f,
            float decayRate = 1f,
            float beamDrain = 1f,
            float lensDrain = 1f,
            float emfRange = 1f,
            float ghostSpeed = 1f,
            float beamedGhostSpeed = 1f,
            bool locksHiddenGhosts = false,
            bool showsEmfDirection = false)
        {
            _captureRate = captureRate;
            _reticleRadius = reticleRadius;
            _decayRate = decayRate;
            _beamDrain = beamDrain;
            _lensDrain = lensDrain;
            _emfRange = emfRange;
            _ghostSpeed = ghostSpeed;
            _beamedGhostSpeed = beamedGhostSpeed;
            _locksHiddenGhosts = locksHiddenGhosts;
            _showsEmfDirection = showsEmfDirection;
        }

        public float CaptureRate => _captureRate;

        public float ReticleRadius => _reticleRadius;

        public float DecayRate => _decayRate;

        public float BeamDrain => _beamDrain;

        public float LensDrain => _lensDrain;

        public float EmfRange => _emfRange;

        public float GhostSpeed => _ghostSpeed;

        // Applies on top of GhostSpeed only while the beam holds the ghost.
        public float BeamedGhostSpeed => _beamedGhostSpeed;

        // The beam keeps its grip on a ghost the lens has revealed even while it blinks out or shrieks itself hidden.
        public bool LocksHiddenGhosts => _locksHiddenGhosts;

        public bool ShowsEmfDirection => _showsEmfDirection;
    }
}
