using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class AudioConfig
    {
        [SerializeField] private AudioClip _ambientDrone;
        [SerializeField, Range(0f, 1f)] private float _ambientDroneVolume = 0.35f;
        [SerializeField] private AudioClip _ambientStatic;
        [SerializeField, Range(0f, 1f)] private float _ambientStaticVolume = 0.18f;
        [SerializeField] private AudioClip _lensOn;
        [SerializeField] private AudioClip _lensOff;
        [SerializeField, Range(0f, 1f)] private float _lensVolume = 0.6f;
        [SerializeField] private AudioClip _beamLoop;
        [SerializeField, Range(0f, 1f)] private float _beamVolume = 0.45f;
        [SerializeField, Min(0.1f)] private float _beamPitchMin = 0.9f;
        [SerializeField, Min(0.1f)] private float _beamPitchMax = 1.35f;
        [SerializeField, Min(0.02f)] private float _beamHapticInterval = 0.15f;
        [SerializeField] private AudioClip _captureSuccess;
        [SerializeField, Range(0f, 1f)] private float _captureVolume = 0.9f;
        [SerializeField] private AudioClip _ghostEscape;
        [SerializeField, Range(0f, 1f)] private float _escapeVolume = 0.8f;
        [SerializeField] private AudioClip _scareSting;
        [SerializeField, Range(0f, 1f)] private float _scareVolume = 1.0f;
        [SerializeField] private AudioClip _teleportWhoosh;
        [SerializeField, Range(0f, 1f)] private float _teleportVolume = 0.8f;
        [SerializeField] private AudioClip _scanComplete;
        [SerializeField, Range(0f, 1f)] private float _scanVolume = 0.7f;
        [SerializeField] private AudioClip _uiClick;
        [SerializeField] private AudioClip _uiBack;
        [SerializeField, Range(0f, 1f)] private float _uiVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _whisperVolume = 0.55f;

        public AudioClip AmbientDrone => _ambientDrone;

        public float AmbientDroneVolume => _ambientDroneVolume;

        public AudioClip AmbientStatic => _ambientStatic;

        public float AmbientStaticVolume => _ambientStaticVolume;

        public AudioClip LensOn => _lensOn;

        public AudioClip LensOff => _lensOff;

        public float LensVolume => _lensVolume;

        public AudioClip BeamLoop => _beamLoop;

        public float BeamVolume => _beamVolume;

        public float BeamPitchMin => _beamPitchMin;

        public float BeamPitchMax => _beamPitchMax;

        public float BeamHapticInterval => _beamHapticInterval;

        public AudioClip CaptureSuccess => _captureSuccess;

        public float CaptureVolume => _captureVolume;

        public AudioClip GhostEscape => _ghostEscape;

        public float EscapeVolume => _escapeVolume;

        public AudioClip ScareSting => _scareSting;

        public float ScareVolume => _scareVolume;

        public AudioClip TeleportWhoosh => _teleportWhoosh;

        public float TeleportVolume => _teleportVolume;

        public AudioClip ScanComplete => _scanComplete;

        public float ScanVolume => _scanVolume;

        public AudioClip UiClick => _uiClick;

        public AudioClip UiBack => _uiBack;

        public float UiVolume => _uiVolume;

        public float WhisperVolume => _whisperVolume;
    }
}
