using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class ToolsConfig
    {
        [SerializeField, Min(0f)] private float _lensDrain = 2f;
        [SerializeField, Range(1f, 90f)] private float _revealAngle = 35f;
        [SerializeField, Min(0.01f)] private float _revealInTime = 0.4f;
        [SerializeField, Min(0.01f)] private float _revealOutTime = 0.8f;

        public ToolsConfig()
        {
        }

        public ToolsConfig(float lensDrain, float revealAngle, float revealInTime, float revealOutTime)
        {
            _lensDrain = lensDrain;
            _revealAngle = revealAngle;
            _revealInTime = revealInTime;
            _revealOutTime = revealOutTime;
        }

        public float LensDrain => _lensDrain;

        public float RevealAngle => _revealAngle;

        public float RevealInTime => _revealInTime;

        public float RevealOutTime => _revealOutTime;
    }
}
