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
        [SerializeField] private AudioClip _dashWhoosh;
        [SerializeField, Range(0f, 1f)] private float _dashVolume = 0.7f;
        [SerializeField] private AudioClip _shriek;
        [SerializeField, Range(0f, 1f)] private float _shriekVolume = 0.95f;
        [SerializeField] private AudioClip _scanComplete;
        [SerializeField, Range(0f, 1f)] private float _scanVolume = 0.7f;
        [SerializeField] private AudioClip _uiClick;
        [SerializeField] private AudioClip _uiBack;
        [SerializeField, Range(0f, 1f)] private float _uiVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _whisperVolume = 0.55f;
        [SerializeField] private AudioClip _ghostReveal;
        [SerializeField, Range(0f, 1f)] private float _revealVolume = 0.7f;
        [SerializeField] private AudioClip _beamLock;
        [SerializeField, Range(0f, 1f)] private float _beamLockVolume = 0.6f;
        [SerializeField] private AudioClip _splashBoot;
        [SerializeField] private AudioClip _splashOff;
        [SerializeField] private AudioClip _bootTick;
        [SerializeField, Range(0f, 1f)] private float _splashVolume = 0.7f;
        [SerializeField] private AudioClip _screenOn;
        [SerializeField, Range(0f, 1f)] private float _transitionVolume = 0.55f;
        [SerializeField] private AudioClip _purchase;
        [SerializeField] private AudioClip _denied;
        [SerializeField, Range(0f, 1f)] private float _storeVolume = 0.7f;
        [SerializeField] private AudioClip _batteryInsert;
        [SerializeField, Range(0f, 1f)] private float _batteryInsertVolume = 0.8f;
        [SerializeField] private AudioClip _pickupEcto;
        [SerializeField] private AudioClip _pickupCase;
        [SerializeField] private AudioClip _pickupCell;
        [SerializeField, Range(0f, 1f)] private float _pickupVolume = 0.8f;
        [SerializeField] private AudioClip _cellBeacon;
        [SerializeField, Range(0f, 1f)] private float _cellBeaconVolume = 0.6f;
        [SerializeField] private AudioClip _emergencyAlarm;
        [SerializeField, Range(0f, 1f)] private float _emergencyVolume = 0.8f;
        [SerializeField] private AudioClip _rewardGranted;
        [SerializeField, Range(0f, 1f)] private float _rewardVolume = 0.8f;
        [SerializeField] private AudioClip _ghostStagger;
        [SerializeField, Range(0f, 1f)] private float _staggerVolume = 0.8f;
        [SerializeField] private AudioClip _ghostSurge;
        [SerializeField, Range(0f, 1f)] private float _surgeVolume = 0.75f;
        [SerializeField, Min(0.02f)] private float _surgeHapticInterval = 0.12f;

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

        public AudioClip DashWhoosh => _dashWhoosh;

        public float DashVolume => _dashVolume;

        public AudioClip Shriek => _shriek;

        public float ShriekVolume => _shriekVolume;

        public AudioClip ScanComplete => _scanComplete;

        public float ScanVolume => _scanVolume;

        public AudioClip UiClick => _uiClick;

        public AudioClip UiBack => _uiBack;

        public float UiVolume => _uiVolume;

        public float WhisperVolume => _whisperVolume;

        public AudioClip GhostReveal => _ghostReveal;

        public float RevealVolume => _revealVolume;

        public AudioClip BeamLock => _beamLock;

        public float BeamLockVolume => _beamLockVolume;

        public AudioClip SplashBoot => _splashBoot;

        public AudioClip SplashOff => _splashOff;

        public AudioClip BootTick => _bootTick;

        public float SplashVolume => _splashVolume;

        public AudioClip ScreenOn => _screenOn;

        public float TransitionVolume => _transitionVolume;

        public AudioClip Purchase => _purchase;

        public AudioClip Denied => _denied;

        public float StoreVolume => _storeVolume;

        public AudioClip BatteryInsert => _batteryInsert;

        public float BatteryInsertVolume => _batteryInsertVolume;

        public AudioClip PickupEcto => _pickupEcto;

        public AudioClip PickupCase => _pickupCase;

        public AudioClip PickupCell => _pickupCell;

        public float PickupVolume => _pickupVolume;

        public AudioClip CellBeacon => _cellBeacon;

        public float CellBeaconVolume => _cellBeaconVolume;

        public AudioClip EmergencyAlarm => _emergencyAlarm;

        public float EmergencyVolume => _emergencyVolume;

        public AudioClip RewardGranted => _rewardGranted;

        public float RewardVolume => _rewardVolume;

        public AudioClip GhostStagger => _ghostStagger;

        public float StaggerVolume => _staggerVolume;

        public AudioClip GhostSurge => _ghostSurge;

        public float SurgeVolume => _surgeVolume;

        // The surge throbs in the hand faster than a normal beam hold.
        public float SurgeHapticInterval => _surgeHapticInterval;
    }
}
