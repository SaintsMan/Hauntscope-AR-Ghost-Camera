using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostDetection
    {
        [SerializeField, Min(0.1f)] private float _emfRange = 6f;

        public GhostDetection()
        {
        }

        public GhostDetection(float emfRange)
        {
            _emfRange = emfRange;
        }

        public float EmfRange => _emfRange;
    }
}
