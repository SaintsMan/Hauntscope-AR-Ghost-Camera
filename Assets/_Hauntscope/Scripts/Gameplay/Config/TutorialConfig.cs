using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class TutorialConfig
    {
        [SerializeField, Min(0f)] private float _walkDistance = 1f;
        [SerializeField, Min(0)] private int _emfLevel = 3;
        [SerializeField, Range(0f, 1f)] private float _revealThreshold = 0.5f;
        [SerializeField, Min(0f)] private float _holdBeamStagger = 3f;

        public TutorialConfig()
        {
        }

        public TutorialConfig(float walkDistance, int emfLevel, float revealThreshold, float holdBeamStagger)
        {
            _walkDistance = walkDistance;
            _emfLevel = emfLevel;
            _revealThreshold = revealThreshold;
            _holdBeamStagger = holdBeamStagger;
        }

        public float WalkDistance => _walkDistance;

        public int EmfLevel => _emfLevel;

        public float RevealThreshold => _revealThreshold;

        // The tutorial ghost has no ability of its own, so the lesson freezes it for the first beam.
        public float HoldBeamStagger => _holdBeamStagger;
    }
}
