using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.33.1: the Spirit Box answers "where" — which way the ghost is and how far.
    [Serializable]
    public sealed class SpiritBoxConfig
    {
        [SerializeField, Min(0)] private int _unlockCaptures = 1;
        [SerializeField, Min(0f)] private float _drain = 0.4f;
        [SerializeField, Min(0f)] private float _firstAnswerDelay = 1.2f;
        [SerializeField, Min(0.1f)] private float _intervalMin = 3.5f;
        [SerializeField, Min(0.1f)] private float _intervalMax = 5.5f;
        [SerializeField, Range(0f, 180f)] private float _aheadAngle = 40f;
        [SerializeField, Range(0f, 180f)] private float _behindAngle = 130f;
        [SerializeField, Min(0f)] private float _closeDistance = 1.5f;
        [SerializeField, Min(0f)] private float _farDistance = 4.5f;
        [SerializeField, Min(0f)] private float _range = 9f;
        [SerializeField, Min(0f)] private float _voiceOffset = 1.2f;
        [SerializeField, Min(0.1f)] private float _answerShowTime = 2.5f;
        [SerializeField] private AudioClip _sweepClip;
        [SerializeField, Range(0f, 1f)] private float _sweepVolume = 0.3f;
        [SerializeField] private AudioClip[] _voiceClips = Array.Empty<AudioClip>();
        [SerializeField, Range(0f, 1f)] private float _voiceVolume = 0.9f;
        [SerializeField] private AudioClip _crackleClip;
        [SerializeField, Range(0f, 1f)] private float _crackleVolume = 0.5f;
        [SerializeField] private AudioClip _toggleClip;
        [SerializeField, Range(0f, 1f)] private float _toggleVolume = 0.6f;

        public SpiritBoxConfig()
        {
        }

        public SpiritBoxConfig(int unlockCaptures, float drain, float firstAnswerDelay, float intervalMin, float intervalMax, float aheadAngle,
            float behindAngle, float closeDistance, float farDistance, float range, float voiceOffset)
        {
            _unlockCaptures = unlockCaptures;
            _drain = drain;
            _firstAnswerDelay = firstAnswerDelay;
            _intervalMin = intervalMin;
            _intervalMax = intervalMax;
            _aheadAngle = aheadAngle;
            _behindAngle = behindAngle;
            _closeDistance = closeDistance;
            _farDistance = farDistance;
            _range = range;
            _voiceOffset = voiceOffset;
        }

        // Captures in total before the agency issues the box.
        public int UnlockCaptures => _unlockCaptures;

        public float Drain => _drain;

        // Switching the box on answers quickly, so the player connects the voice with the button.
        public float FirstAnswerDelay => _firstAnswerDelay;

        public float IntervalMin => _intervalMin;

        public float IntervalMax => _intervalMax;

        // Flat angle from where the camera looks: within this the ghost is ahead, beyond BehindAngle it is behind.
        public float AheadAngle => _aheadAngle;

        public float BehindAngle => _behindAngle;

        public float CloseDistance => _closeDistance;

        public float FarDistance => _farDistance;

        // Beyond this the box only crackles.
        public float Range => _range;

        // The voice is played this far from the camera towards the ghost, so the direction is heard, not just read.
        public float VoiceOffset => _voiceOffset;

        public float AnswerShowTime => _answerShowTime;

        public AudioClip SweepClip => _sweepClip;

        public float SweepVolume => _sweepVolume;

        public AudioClip[] VoiceClips => _voiceClips;

        public float VoiceVolume => _voiceVolume;

        public AudioClip CrackleClip => _crackleClip;

        public float CrackleVolume => _crackleVolume;

        public AudioClip ToggleClip => _toggleClip;

        public float ToggleVolume => _toggleVolume;
    }
}
