using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.33.3: the thermal camera answers "behind what" — the ghost's cold shape through furniture and hiding places.
    [Serializable]
    public sealed class ThermalVisionConfig
    {
        [SerializeField, Min(0)] private int _unlockCaptures = 4;
        [SerializeField, Min(0f)] private float _drain = 0.8f;
        [SerializeField, Min(0f)] private float _range = 4f;
        [SerializeField, Min(0.01f)] private float _fadeTime = 0.25f;
        [SerializeField] private string _labelKey = "hud.view.thermal";
        [SerializeField] private Material _filter;
        [SerializeField] private AudioClip _onClip;
        [SerializeField, Range(0f, 1f)] private float _onVolume = 0.6f;

        public ThermalVisionConfig()
        {
        }

        public ThermalVisionConfig(int unlockCaptures, float drain, float range, float fadeTime)
        {
            _unlockCaptures = unlockCaptures;
            _drain = drain;
            _range = range;
            _fadeTime = fadeTime;
        }

        public int UnlockCaptures => _unlockCaptures;

        public float Drain => _drain;

        public float Range => _range;

        public float FadeTime => _fadeTime;

        public string LabelKey => _labelKey;

        public Material Filter => _filter;

        public AudioClip OnClip => _onClip;

        public float OnVolume => _onVolume;
    }
}
