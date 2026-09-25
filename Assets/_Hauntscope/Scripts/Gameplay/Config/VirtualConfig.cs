using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class VirtualConfig
    {
        [SerializeField, Min(0.1f)] private float _walkSpeed = 1.3f;
        [SerializeField, Min(1f)] private float _lookSensitivity = 160f;
        [SerializeField, Min(0.5f)] private float _eyeHeight = 1.55f;
        [SerializeField, Range(0f, 89f)] private float _pitchLimit = 70f;
        [SerializeField, Min(0.05f)] private float _bodyRadius = 0.25f;
        [SerializeField, Min(0f)] private float _stepHeight = 0.35f;

        public VirtualConfig()
        {
        }

        public VirtualConfig(float walkSpeed, float lookSensitivity, float eyeHeight, float pitchLimit, float bodyRadius, float stepHeight)
        {
            _walkSpeed = walkSpeed;
            _lookSensitivity = lookSensitivity;
            _eyeHeight = eyeHeight;
            _pitchLimit = pitchLimit;
            _bodyRadius = bodyRadius;
            _stepHeight = stepHeight;
        }

        public float WalkSpeed => _walkSpeed;

        public float LookSensitivity => _lookSensitivity;

        public float EyeHeight => _eyeHeight;

        public float PitchLimit => _pitchLimit;

        public float BodyRadius => _bodyRadius;

        public float StepHeight => _stepHeight;
    }
}
