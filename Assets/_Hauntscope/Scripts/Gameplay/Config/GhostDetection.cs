using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostDetection
    {
        [SerializeField, Min(0.1f)] private float _emfRange = 6f;
        [SerializeField, Min(0.1f)] private float _revealRange = 3f;

        public GhostDetection()
        {
        }

        public GhostDetection(float emfRange, float revealRange)
        {
            _emfRange = emfRange;
            _revealRange = revealRange;
        }

        public float EmfRange => _emfRange;

        public float RevealRange => _revealRange;
    }
}
