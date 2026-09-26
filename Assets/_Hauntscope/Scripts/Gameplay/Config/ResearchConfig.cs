using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class ResearchConfig
    {
        [SerializeField, Min(1)] private int _capturesToDeclassify = 5;
        [SerializeField, Min(0f)] private float _declassifiedBonus = 0.25f;
        [SerializeField, Min(0)] private int _maxPhotoEvidence = 2;

        public ResearchConfig()
        {
        }

        public ResearchConfig(int capturesToDeclassify, float declassifiedBonus, int maxPhotoEvidence = 2)
        {
            _maxPhotoEvidence = maxPhotoEvidence;
            _capturesToDeclassify = capturesToDeclassify;
            _declassifiedBonus = declassifiedBonus;
        }

        public int CapturesToDeclassify => _capturesToDeclassify;

        // Extra share of the capture reward for a ghost whose file is declassified.
        public float DeclassifiedBonus => _declassifiedBonus;

        // How many good photos of a ghost may stand in for captures on the way to declassifying its file.
        public int MaxPhotoEvidence => _maxPhotoEvidence;
    }
}
