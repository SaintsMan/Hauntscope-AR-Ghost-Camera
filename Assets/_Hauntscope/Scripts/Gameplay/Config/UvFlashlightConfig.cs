using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.33.4: the UV flashlight answers "where did it go" — glowing ectoplasm the ghost left behind.
    [Serializable]
    public sealed class UvFlashlightConfig
    {
        [SerializeField, Min(0)] private int _unlockCaptures = 6;
        [SerializeField, Min(0f)] private float _drain = 0.8f;
        [SerializeField, Min(0f)] private float _range = 3f;
        [SerializeField, Range(1f, 89f)] private float _coneAngle = 28f;
        [SerializeField, Range(0f, 30f)] private float _coneSoftness = 6f;
        [SerializeField, Min(0.05f)] private float _trailSpacing = 0.45f;
        [SerializeField, Min(1f)] private float _trailLifetime = 20f;
        [SerializeField, Range(0f, 1f)] private float _fadeStart = 0.6f;
        [SerializeField, Min(8)] private int _trailCapacity = 64;
        [SerializeField] private string _labelKey = "hud.view.uv";
        [SerializeField] private Material _filter;
        [SerializeField] private AudioClip _onClip;
        [SerializeField, Range(0f, 1f)] private float _onVolume = 0.6f;

        public UvFlashlightConfig()
        {
        }

        public UvFlashlightConfig(int unlockCaptures, float drain, float range, float coneAngle, float coneSoftness, float trailSpacing,
            float trailLifetime, float fadeStart, int trailCapacity)
        {
            _unlockCaptures = unlockCaptures;
            _drain = drain;
            _range = range;
            _coneAngle = coneAngle;
            _coneSoftness = coneSoftness;
            _trailSpacing = trailSpacing;
            _trailLifetime = trailLifetime;
            _fadeStart = fadeStart;
            _trailCapacity = trailCapacity;
        }

        public int UnlockCaptures => _unlockCaptures;

        public float Drain => _drain;

        // How far the beam reaches, and its half-angle; the last ConeSoftness degrees fade out at the rim.
        public float Range => _range;

        public float ConeAngle => _coneAngle;

        public float ConeSoftness => _coneSoftness;

        // A drip is left every this many metres of the ghost's path.
        public float TrailSpacing => _trailSpacing;

        // Marks last this long; they start to fade at FadeStart of it.
        public float TrailLifetime => _trailLifetime;

        public float FadeStart => _fadeStart;

        public int TrailCapacity => _trailCapacity;

        public string LabelKey => _labelKey;

        public Material Filter => _filter;

        public AudioClip OnClip => _onClip;

        public float OnVolume => _onVolume;
    }
}
