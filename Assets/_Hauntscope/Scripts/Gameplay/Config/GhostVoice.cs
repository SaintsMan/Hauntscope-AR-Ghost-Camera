using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // A ghost's own sounds (GDD 5.32). Any clip left empty falls back to the shared one in AudioConfig.
    [Serializable]
    public sealed class GhostVoice
    {
        [SerializeField] private AudioClip _whisper;
        [SerializeField] private AudioClip _ability;
        [SerializeField] private AudioClip _scare;
        [SerializeField] private AudioClip _capture;
        [SerializeField] private AudioClip _escape;
        [SerializeField] private AudioClip _stagger;

        public GhostVoice()
        {
        }

        public GhostVoice(AudioClip whisper, AudioClip ability = null, AudioClip scare = null, AudioClip capture = null,
            AudioClip escape = null, AudioClip stagger = null)
        {
            _whisper = whisper;
            _ability = ability;
            _scare = scare;
            _capture = capture;
            _escape = escape;
            _stagger = stagger;
        }

        // Looped, positioned at the ghost: how the player finds it by ear.
        public AudioClip Whisper => _whisper;

        // The sound of its trick: a teleport, a dash, a shriek, a knock, a chain.
        public AudioClip Ability => _ability;

        // Its scream when it lunges at the camera.
        public AudioClip Scare => _scare;

        // Its last cry as the beam pulls it in.
        public AudioClip Capture => _capture;

        public AudioClip Escape => _escape;

        // Its grunt when it is left stunned.
        public AudioClip Stagger => _stagger;
    }
}
