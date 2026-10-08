using System;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // The agency's help for a new agent (GDD 5.36): the full set on the first hunt, fading out catch by catch.
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
            passiveDrain: 0.6f,
            revealFade: 2f);

        public RookieConfig()
        {
        }

        public RookieConfig(int fadeCaptures, HuntModifierSet assist)
        {
            _fadeCaptures = fadeCaptures;
            _assist = assist;
        }

        // Total catches after which the help is gone.
        public int FadeCaptures => _fadeCaptures;
        public HuntModifierSet Assist => _assist;
    }
}
