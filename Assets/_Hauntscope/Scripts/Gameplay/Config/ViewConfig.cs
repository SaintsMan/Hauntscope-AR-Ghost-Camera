using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // The camera modes behind the MODE button (GDD 5.33): what they share, and each mode's own settings.
    [Serializable]
    public sealed class ViewConfig
    {
        [SerializeField] private NightVisionConfig _nightVision = new NightVisionConfig();
        [SerializeField] private ThermalVisionConfig _thermal = new ThermalVisionConfig();
        [SerializeField] private UvFlashlightConfig _uv = new UvFlashlightConfig();
        [SerializeField] private AudioClip _offClip;
        [SerializeField, Range(0f, 1f)] private float _offVolume = 0.5f;

        public NightVisionConfig NightVision => _nightVision;

        public ThermalVisionConfig Thermal => _thermal;

        public UvFlashlightConfig Uv => _uv;

        // Back to the plain picture.
        public AudioClip OffClip => _offClip;

        public float OffVolume => _offVolume;
    }
}
