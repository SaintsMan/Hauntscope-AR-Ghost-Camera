using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class CaptureConfig
    {
        [SerializeField, Min(1f)] private float _staggerCaptureMultiplier = 2f;
        [SerializeField, Min(0f)] private float _staggerSink = 0.15f;
        [SerializeField, Min(0.01f)] private float _staggerBlendTime = 0.15f;
        [SerializeField, Min(0f)] private float _closeDistance = 1f;
        [SerializeField, Min(0f)] private float _farDistance = 2.5f;
        [SerializeField, Min(0f)] private float _closeCaptureMultiplier = 1.5f;
        [SerializeField, Min(0f)] private float _farCaptureMultiplier = 0.75f;
        [SerializeField, Range(0f, 1f)] private float _surgeThreshold = 0.85f;
        [SerializeField, Range(0f, 1f)] private float _surgeRearm = 0.6f;
        [SerializeField, Min(0.1f)] private float _surgeDuration = 1.6f;
        [SerializeField, Min(0.05f)] private float _surgeJerkInterval = 0.3f;
        [SerializeField, Min(0f)] private float _surgeCaptureScale = 0.6f;
        [SerializeField, Min(0f)] private float _surgeDecayScale = 2f;
        [SerializeField, Range(0f, 1f)] private float _scareProgressLoss = 0.3f;

        public CaptureConfig()
        {
        }

        public CaptureConfig(
            float staggerCaptureMultiplier,
            float staggerSink,
            float staggerBlendTime,
            float closeDistance,
            float farDistance,
            float closeCaptureMultiplier,
            float farCaptureMultiplier,
            float surgeThreshold,
            float surgeRearm,
            float surgeDuration,
            float surgeJerkInterval,
            float surgeCaptureScale,
            float surgeDecayScale,
            float scareProgressLoss)
        {
            _staggerCaptureMultiplier = staggerCaptureMultiplier;
            _staggerSink = staggerSink;
            _staggerBlendTime = staggerBlendTime;
            _closeDistance = closeDistance;
            _farDistance = farDistance;
            _closeCaptureMultiplier = closeCaptureMultiplier;
            _farCaptureMultiplier = farCaptureMultiplier;
            _surgeThreshold = surgeThreshold;
            _surgeRearm = surgeRearm;
            _surgeDuration = surgeDuration;
            _surgeJerkInterval = surgeJerkInterval;
            _surgeCaptureScale = surgeCaptureScale;
            _surgeDecayScale = surgeDecayScale;
            _scareProgressLoss = scareProgressLoss;
        }

        public float StaggerCaptureMultiplier => _staggerCaptureMultiplier;

        // How far a staggered ghost sags, in metres: the "winded" pose that tells the player to strike.
        public float StaggerSink => _staggerSink;

        public float StaggerBlendTime => _staggerBlendTime;

        public float CloseDistance => _closeDistance;

        public float FarDistance => _farDistance;

        public float CloseCaptureMultiplier => _closeCaptureMultiplier;

        public float FarCaptureMultiplier => _farCaptureMultiplier;

        public float SurgeThreshold => _surgeThreshold;

        public float SurgeRearm => _surgeRearm;

        public float SurgeDuration => _surgeDuration;

        public float SurgeJerkInterval => _surgeJerkInterval;

        public float SurgeCaptureScale => _surgeCaptureScale;

        public float SurgeDecayScale => _surgeDecayScale;

        public float ScareProgressLoss => _scareProgressLoss;
    }
}
