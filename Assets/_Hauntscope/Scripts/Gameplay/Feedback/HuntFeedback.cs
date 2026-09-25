using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // Turns hunt events into sound, haptics and VFX; gameplay classes stay unaware of any feedback.
    public sealed class HuntFeedback : IStartable, ITickable, IDisposable
    {
        private readonly HuntSession _session;
        private readonly Toolbelt _toolbelt;
        private readonly RoomCalibration _calibration;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly IVfxPlayer _vfx;
        private readonly AudioConfig _audio;

        private Ghost _ghost;
        private ISfxLoop _whisper;
        private ISfxLoop _beam;
        private bool _wasCaptured;
        private bool _wasEscaped;
        private bool _wasCalibrated;
        private float _timeUntilBeamPulse;

        public HuntFeedback(
            HuntSession session,
            Toolbelt toolbelt,
            RoomCalibration calibration,
            ISfxPlayer sfx,
            IHaptics haptics,
            IVfxPlayer vfx,
            AudioConfig audio)
        {
            _session = session;
            _toolbelt = toolbelt;
            _calibration = calibration;
            _sfx = sfx;
            _haptics = haptics;
            _vfx = vfx;
            _audio = audio;
        }

        private Color RimColor => _session.GhostData != null ? _session.GhostData.RimColor : Color.cyan;

        public void Start()
        {
            _session.Ghost.Changed += OnGhostChanged;
            _session.Scared += OnScared;
            _toolbelt.Lens.IsActive.Changed += OnLensChanged;
            _toolbelt.Beam.IsActive.Changed += OnBeamChanged;
            _calibration.Progress.Changed += OnCalibrationChanged;
            _wasCalibrated = _calibration.IsComplete;
        }

        void ITickable.Tick()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (_ghost == null)
                return;

            _whisper?.SetPosition(_ghost.Position);
            _beam?.SetPitch(Mathf.Lerp(_audio.BeamPitchMin, _audio.BeamPitchMax, _toolbelt.Beam.Progress.Value));

            if (_ghost.IsBeamed)
            {
                _timeUntilBeamPulse -= deltaTime;
                if (_timeUntilBeamPulse <= 0f)
                {
                    _haptics.Play(HapticStrength.Light);
                    _timeUntilBeamPulse = _audio.BeamHapticInterval;
                }
            }

            if (_ghost.IsCaptured && !_wasCaptured)
                OnCaptureStarted();
            if (_ghost.IsEscaped && !_wasEscaped)
                OnEscapeStarted();

            _wasCaptured = _ghost.IsCaptured;
            _wasEscaped = _ghost.IsEscaped;
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= OnGhostChanged;
            _session.Scared -= OnScared;
            _toolbelt.Lens.IsActive.Changed -= OnLensChanged;
            _toolbelt.Beam.IsActive.Changed -= OnBeamChanged;
            _calibration.Progress.Changed -= OnCalibrationChanged;
            DetachGhost();
            StopBeam();
        }

        private void OnGhostChanged(Ghost ghost)
        {
            DetachGhost();
            _ghost = ghost;
            _wasCaptured = false;
            _wasEscaped = false;
            if (_ghost == null)
                return;

            _ghost.Teleported += OnTeleported;
            var clip = _session.GhostData != null ? _session.GhostData.WhisperClip : null;
            _whisper = _sfx.PlayLoop(clip, _audio.WhisperVolume, true);
            _whisper?.SetPosition(_ghost.Position);
        }

        private void DetachGhost()
        {
            if (_ghost != null)
                _ghost.Teleported -= OnTeleported;
            _whisper?.Stop();
            _whisper = null;
            _ghost = null;
        }

        private void OnCaptureStarted()
        {
            _whisper?.Stop();
            _whisper = null;
            StopBeam();
            _sfx.Play2D(_audio.CaptureSuccess, _audio.CaptureVolume, 1f);
            _haptics.Play(HapticStrength.Heavy);
            _vfx.Play(VfxId.CaptureSpiral, _ghost.Position, RimColor);
        }

        private void OnEscapeStarted()
        {
            _whisper?.Stop();
            _whisper = null;
            _sfx.Play3D(_audio.GhostEscape, _ghost.Position, _audio.EscapeVolume);
        }

        private void OnTeleported(Vector3 from, Vector3 to)
        {
            _sfx.Play3D(_audio.TeleportWhoosh, to, _audio.TeleportVolume);
            _vfx.Play(VfxId.TeleportFlash, from, RimColor);
            _vfx.Play(VfxId.TeleportFlash, to, RimColor);
        }

        private void OnScared()
        {
            _sfx.Play2D(_audio.ScareSting, _audio.ScareVolume, 1f);
            _haptics.Play(HapticStrength.Heavy);
        }

        private void OnLensChanged(bool active)
        {
            _sfx.Play2D(active ? _audio.LensOn : _audio.LensOff, _audio.LensVolume, 1f);
        }

        private void OnBeamChanged(bool active)
        {
            if (active && _beam == null)
                _beam = _sfx.PlayLoop(_audio.BeamLoop, _audio.BeamVolume, false);
            else if (!active)
                StopBeam();
        }

        private void StopBeam()
        {
            _beam?.Stop();
            _beam = null;
        }

        private void OnCalibrationChanged(float progress)
        {
            if (_calibration.IsComplete && !_wasCalibrated)
                _sfx.Play2D(_audio.ScanComplete, _audio.ScanVolume, 1f);
            _wasCalibrated = _calibration.IsComplete;
        }
    }
}
