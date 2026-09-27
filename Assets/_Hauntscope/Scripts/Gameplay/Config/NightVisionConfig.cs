using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.33.2: night vision answers "where is it now" without revealing — a faint shape the ghost does not notice.
    [Serializable]
    public sealed class NightVisionConfig
    {
        [SerializeField, Min(0)] private int _unlockCaptures = 2;
        [SerializeField, Min(0f)] private float _drain = 0.3f;
        [SerializeField, Min(0f)] private float _range = 5f;
        // Below the ghost view's trail threshold, so the shape shows but the ghost's trail does not.
        [SerializeField, Range(0f, 1f)] private float _glimpse = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _farGlimpseScale = 0.5f;
        [SerializeField, Min(0.01f)] private float _fadeTime = 0.35f;
        [SerializeField] private string _labelKey = "hud.view.night";
        [SerializeField] private Material _filter;
        [SerializeField] private AudioClip _onClip;
        [SerializeField, Range(0f, 1f)] private float _onVolume = 0.6f;

        public NightVisionConfig()
        {
        }

        public NightVisionConfig(int unlockCaptures, float drain, float range, float glimpse, float farGlimpseScale, float fadeTime)
        {
            _unlockCaptures = unlockCaptures;
            _drain = drain;
            _range = range;
            _glimpse = glimpse;
            _farGlimpseScale = farGlimpseScale;
            _fadeTime = fadeTime;
        }

        public int UnlockCaptures => _unlockCaptures;

        public float Drain => _drain;

        public float Range => _range;

        // How visible the ghost is up close; at the edge of the range it is this times FarGlimpseScale.
        public float Glimpse => _glimpse;

        public float FarGlimpseScale => _farGlimpseScale;

        public float FadeTime => _fadeTime;

        public string LabelKey => _labelKey;

        public Material Filter => _filter;

        public AudioClip OnClip => _onClip;

        public float OnVolume => _onVolume;
    }
}
