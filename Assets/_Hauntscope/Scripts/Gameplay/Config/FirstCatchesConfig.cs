using System;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // The first catches on top of the fading rookie help (GDD 5.36): 1.0.6 still lost most new agents between the
    // radar and the lens (emf_found 336 -> ghost_seen 136 -> beam_lock 59). The first hunt gets the full set (the
    // battery holds, the lens sees wider and farther, an arrow points at the ghost, the beam pins it close), the
    // next few get less of it each catch, and after Captures catches only the ordinary rookie help is left.
    // Every knob has a Remote Config key (RemoteKeys.RookieEasy*); these values are the fallback.
    [Serializable]
    public sealed class FirstCatchesConfig
    {
        [SerializeField, Min(0)] private int _captures = 4;
        [SerializeField] private HuntModifierSet _assist = new HuntModifierSet(
            captureRate: 1.6f,
            reticleRadius: 1.5f,
            decayRate: 0.4f,
            beamDrain: 0f,
            lensDrain: 0f,
            emfRange: 1.5f,
            ghostSpeed: 0.6f,
            beamedGhostSpeed: 0.3f,
            locksHiddenGhosts: true,
            showsEmfDirection: true,
            passiveDrain: 0f,
            revealFade: 2f);
        [SerializeField, Min(1f)] private float _lensAngle = 1.6f;
        [SerializeField, Min(1f)] private float _lensRange = 1.5f;
        [SerializeField, Min(0f)] private float _leashDistance = 1.5f;

        public FirstCatchesConfig()
        {
        }

        public FirstCatchesConfig(int captures, HuntModifierSet assist, float lensAngle = 1f, float lensRange = 1f, float leashDistance = 0f)
        {
            _captures = captures;
            _assist = assist;
            _lensAngle = lensAngle;
            _lensRange = lensRange;
            _leashDistance = leashDistance;
        }

        // Catches the extra help lasts for: full on the first hunt, gone after this many catches (0 = off).
        public int Captures => _captures;

        // Drain multipliers of 0 keep the battery full on the first hunt; flags (the EMF arrow, the beam's grip on a
        // flickering ghost) last while the help is at least half strength.
        public HuntModifierSet Assist => _assist;

        // Multiplies the lens' reveal angle (30 degrees in GameConfig).
        public float LensAngle => _lensAngle;

        // Multiplies how far the lens sees a ghost (the wisp's 3 m).
        public float LensRange => _lensRange;

        // While the beam is on it, the first ghost stays within this distance of the camera; it eases back to the
        // ordinary rookie leash catch by catch.
        public float LeashDistance => _leashDistance;
    }
}
