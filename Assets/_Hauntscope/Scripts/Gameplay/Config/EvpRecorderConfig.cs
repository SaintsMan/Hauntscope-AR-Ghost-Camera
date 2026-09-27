using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.33.5: the EVP recorder answers "who is it" — the ghost's voice says its name on the tape.
    [Serializable]
    public sealed class EvpRecorderConfig
    {
        [SerializeField, Min(0)] private int _unlockCaptures = 9;
        [SerializeField, Range(0f, 1f)] private float _cost = 0.03f;
        [SerializeField, Min(0.1f)] private float _recordTime = 4f;
        [SerializeField, Min(0.1f)] private float _playbackTime = 2.6f;
        [SerializeField, Min(0f)] private float _cooldown = 8f;
        [SerializeField, Min(0f)] private float _range = 4f;
        [SerializeField, Range(0f, 1f)] private float _ambientDuck = 0.35f;
        [SerializeField] private AudioClip _startClip;
        [SerializeField, Range(0f, 1f)] private float _startVolume = 0.7f;
        [SerializeField] private AudioClip _tapeClip;
        [SerializeField, Range(0f, 1f)] private float _tapeVolume = 0.35f;
        [SerializeField] private AudioClip _staticClip;
        [SerializeField, Range(0f, 1f)] private float _staticVolume = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _voiceVolume = 1f;
        [SerializeField] private AudioClip _stopClip;
        [SerializeField, Range(0f, 1f)] private float _stopVolume = 0.6f;

        public EvpRecorderConfig()
        {
        }

        public EvpRecorderConfig(int unlockCaptures, float cost, float recordTime, float playbackTime, float cooldown, float range)
        {
            _unlockCaptures = unlockCaptures;
            _cost = cost;
            _recordTime = recordTime;
            _playbackTime = playbackTime;
            _cooldown = cooldown;
            _range = range;
        }

        public int UnlockCaptures => _unlockCaptures;

        // A share of a full battery, spent at once when the tape starts.
        public float Cost => _cost;

        public float RecordTime => _recordTime;

        // How long the tape plays back before the recorder rests.
        public float PlaybackTime => _playbackTime;

        // Rest after playback before the next take.
        public float Cooldown => _cooldown;

        // The ghost has to come this close at some moment of the take for its voice to be on the tape.
        public float Range => _range;

        // The room's hum drops to this share while the tape runs, so the take is heard.
        public float AmbientDuck => _ambientDuck;

        public AudioClip StartClip => _startClip;

        public float StartVolume => _startVolume;

        public AudioClip TapeClip => _tapeClip;

        public float TapeVolume => _tapeVolume;

        public AudioClip StaticClip => _staticClip;

        public float StaticVolume => _staticVolume;

        public float VoiceVolume => _voiceVolume;

        public AudioClip StopClip => _stopClip;

        public float StopVolume => _stopVolume;
    }
}
