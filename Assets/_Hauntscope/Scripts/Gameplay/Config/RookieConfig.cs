using System;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // The agency's help for a new agent (GDD 5.36): the full set on the first hunt, fading out catch by catch.
    // Every knob here has a Remote Config key (RemoteKeys.Rookie*); these values are the fallback.
    [Serializable]
    public sealed class RookieConfig
    {
        [SerializeField, Min(1)] private int _fadeCaptures = 12;
        [SerializeField] private HuntModifierSet _assist = new HuntModifierSet(
            captureRate: 1.5f,
            reticleRadius: 1.2f,
            decayRate: 0.5f,
            beamDrain: 0.65f,
            lensDrain: 0.6f,
            ghostSpeed: 0.8f,
            beamedGhostSpeed: 0.35f,
            passiveDrain: 0.6f,
            revealFade: 2f);
        [SerializeField, Min(0f)] private float _leashDistance = 2.5f;
        [SerializeField, Min(0)] private int _leashCaptures = 6;
        [SerializeField, Min(0f)] private float _leashGrowth = 0.1f;
        [SerializeField, Min(0)] private int _freeChargeHunts = 3;
        [SerializeField, Range(0f, 1f)] private float _freeCharge = 0.6f;
        [SerializeField] private FirstCatchesConfig _firstCatches = new FirstCatchesConfig();

        public RookieConfig()
        {
        }

        public RookieConfig(int fadeCaptures, HuntModifierSet assist, float leashDistance = 0f, int leashCaptures = 0,
            int freeChargeHunts = 0, float freeCharge = 0f, float leashGrowth = 0f, FirstCatchesConfig firstCatches = null)
        {
            _firstCatches = firstCatches ?? new FirstCatchesConfig(0, new HuntModifierSet());
            _leashGrowth = leashGrowth;
            _fadeCaptures = fadeCaptures;
            _assist = assist;
            _leashDistance = leashDistance;
            _leashCaptures = leashCaptures;
            _freeChargeHunts = freeChargeHunts;
            _freeCharge = freeCharge;
        }

        // Total catches after which the help is gone.
        public int FadeCaptures => _fadeCaptures;
        public HuntModifierSet Assist => _assist;

        // While the beam is on it, a fleeing ghost does not get farther than this from the camera (0 = no leash):
        // it stays inside the lens' reveal range, so a steady aim is enough to finish the catch.
        public float LeashDistance => _leashDistance;

        // Catches the leash lasts for.
        public int LeashCaptures => _leashCaptures;

        // Metres the leash gets longer with every catch, so the ghost slips out of the lens a little more each hunt
        // instead of all at once when the leash ends.
        public float LeashGrowth => _leashGrowth;

        // Hunts (counting the current one) whose dead battery is recharged for free once, before any ad or spare.
        public int FreeChargeHunts => _freeChargeHunts;

        // Fraction of the battery the free charge restores.
        public float FreeCharge => _freeCharge;

        // The stronger help of the very first catches, on top of everything above.
        public FirstCatchesConfig FirstCatches => _firstCatches;
    }
}
