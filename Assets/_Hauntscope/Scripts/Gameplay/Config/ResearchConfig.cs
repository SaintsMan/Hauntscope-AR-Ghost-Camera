using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class ResearchConfig
    {
        [SerializeField, Min(1)] private int _capturesToDeclassify = 5;
        [SerializeField, Min(0f)] private float _declassifiedBonus = 0.25f;

        public ResearchConfig()
        {
        }

        public ResearchConfig(int capturesToDeclassify, float declassifiedBonus)
        {
            _capturesToDeclassify = capturesToDeclassify;
            _declassifiedBonus = declassifiedBonus;
        }

        public int CapturesToDeclassify => _capturesToDeclassify;

        // Extra share of the capture reward for a ghost whose file is declassified.
        public float DeclassifiedBonus => _declassifiedBonus;
    }
}
