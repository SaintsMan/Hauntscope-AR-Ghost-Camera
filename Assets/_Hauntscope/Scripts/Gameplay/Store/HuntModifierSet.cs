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
        [SerializeField, Min(0f)] private float _passiveDrain = 1f;
        [SerializeField, Min(0f)] private float _revealFade = 1f;

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
            bool showsEmfDirection = false,
            float passiveDrain = 1f,
            float revealFade = 1f)
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
            _passiveDrain = passiveDrain;
            _revealFade = revealFade;
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
        public float PassiveDrain => _passiveDrain;
        // Stretches how long a revealed ghost takes to fade once it leaves the lens: 2 = it lingers twice as long.
        public float RevealFade => _revealFade;

        // The same set at partial strength: 0 changes nothing, 1 is the full set. Flags need at least half strength.
        public HuntModifierSet Scaled(float strength)
        {
            strength = Mathf.Clamp01(strength);
            return new HuntModifierSet(
                Mathf.Lerp(1f, _captureRate, strength),
                Mathf.Lerp(1f, _reticleRadius, strength),
                Mathf.Lerp(1f, _decayRate, strength),
                Mathf.Lerp(1f, _beamDrain, strength),
                Mathf.Lerp(1f, _lensDrain, strength),
                Mathf.Lerp(1f, _emfRange, strength),
                Mathf.Lerp(1f, _ghostSpeed, strength),
                Mathf.Lerp(1f, _beamedGhostSpeed, strength),
                _locksHiddenGhosts && strength >= 0.5f,
                _showsEmfDirection && strength >= 0.5f,
                Mathf.Lerp(1f, _passiveDrain, strength),
                Mathf.Lerp(1f, _revealFade, strength));
        }
    }
}
