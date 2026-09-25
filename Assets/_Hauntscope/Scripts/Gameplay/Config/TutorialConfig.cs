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
        [SerializeField, Range(0f, 1f)] private float _beamProgress = 0.25f;

        public TutorialConfig()
        {
        }

        public TutorialConfig(float walkDistance, int emfLevel, float revealThreshold, float beamProgress)
        {
            _walkDistance = walkDistance;
            _emfLevel = emfLevel;
            _revealThreshold = revealThreshold;
            _beamProgress = beamProgress;
        }

        public float WalkDistance => _walkDistance;

        public int EmfLevel => _emfLevel;

        public float RevealThreshold => _revealThreshold;

        public float BeamProgress => _beamProgress;
    }
}
