using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.34: the generated soundtrack and how the hunt's layers answer the hunt.
    [Serializable]
    public sealed class MusicConfig
    {
        [SerializeField] private AudioClip _menuTheme;
        [SerializeField, Range(0f, 1f)] private float _menuVolume = 0.6f;
        // In the order of HuntMusicLayer: drone, pulse, heart, arp, drums.
        [SerializeField] private AudioClip[] _huntLayers = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float _huntVolume = 0.7f;
        [SerializeField] private AudioClip _capturedStinger;
        [SerializeField] private AudioClip _escapedStinger;
        [SerializeField] private AudioClip _surgeStinger;
        [SerializeField, Range(0f, 1f)] private float _stingerVolume = 0.8f;
        [SerializeField, Min(0.05f)] private float _fadeTime = 1.2f;
        [SerializeField, Range(0.5f, 1.5f)] private float _witchingHourPitch = 0.94f;
        [SerializeField, Range(0f, 1f)] private float _scanDrone = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _searchPulse = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _heartFrom = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _resultDrone = 0.45f;
        [SerializeField, Range(0f, 1f)] private float _pauseDuck = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _recordingDuck = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _prankPulse = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _prankArp = 0.35f;

        public MusicConfig()
        {
        }

        public MusicConfig(float scanDrone, float searchPulse, float heartFrom, float resultDrone, float pauseDuck, float recordingDuck,
            float prankPulse, float prankArp)
        {
            _scanDrone = scanDrone;
            _searchPulse = searchPulse;
            _heartFrom = heartFrom;
            _resultDrone = resultDrone;
            _pauseDuck = pauseDuck;
            _recordingDuck = recordingDuck;
            _prankPulse = prankPulse;
            _prankArp = prankArp;
        }

        public AudioClip MenuTheme => _menuTheme;

        public float MenuVolume => _menuVolume;

        public AudioClip[] HuntLayers => _huntLayers;

        public float HuntVolume => _huntVolume;

        public AudioClip CapturedStinger => _capturedStinger;

        public AudioClip EscapedStinger => _escapedStinger;

        public AudioClip SurgeStinger => _surgeStinger;

        public float StingerVolume => _stingerVolume;

        // How long a layer takes to fade fully in or out.
        public float FadeTime => _fadeTime;

        // After dark the whole score plays lower and slower (GDD 5.28).
        public float WitchingHourPitch => _witchingHourPitch;

        // While the room is still being scanned, only the drone, this loud.
        public float ScanDrone => _scanDrone;

        // The pulse while searching far away; it grows to full as the EMF climbs.
        public float SearchPulse => _searchPulse;

        // The heart comes in from this share of the EMF scale.
        public float HeartFrom => _heartFrom;

        // Under the result card, the drone alone at this level.
        public float ResultDrone => _resultDrone;

        public float PauseDuck => _pauseDuck;

        public float RecordingDuck => _recordingDuck;

        // A prank photo gets a calm bed: the drone, a soft pulse and a little arpeggio.
        public float PrankPulse => _prankPulse;

        public float PrankArp => _prankArp;
    }
}
