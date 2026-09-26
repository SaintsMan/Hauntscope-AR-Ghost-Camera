using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
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
        private readonly VfxConfig _vfxConfig;
        private readonly HuntPause _pause;
        private readonly GameSettings _settings;

        private Ghost _ghost;
        private ISfxLoop _whisper;
        private ISfxLoop _beam;
        private bool _wasCaptured;
        private bool _wasEscaped;
        private bool _wasCalibrated;
        private bool _wasRevealed;
        private float _timeUntilBeamPulse;

        public HuntFeedback(
            HuntSession session,
            Toolbelt toolbelt,
            RoomCalibration calibration,
            ISfxPlayer sfx,
            IHaptics haptics,
            IVfxPlayer vfx,
            AudioConfig audio,
            VfxConfig vfxConfig,
            HuntPause pause,
            GameSettings settings)
        {
            _settings = settings;
            _vfxConfig = vfxConfig;
            _pause = pause;
            _session = session;
            _toolbelt = toolbelt;
            _calibration = calibration;
            _sfx = sfx;
            _haptics = haptics;
            _vfx = vfx;
            _audio = audio;
        }

        private Color RimColor => _session.GhostData != null ? _session.GhostData.RimColor : Color.cyan;

        private GhostVoice Voice => _session.GhostData != null ? _session.GhostData.Voice : null;

        public void Start()
        {
            _session.Ghost.Changed += OnGhostChanged;
            _session.Scared += OnScared;
            _toolbelt.Lens.IsActive.Changed += OnLensChanged;
            _toolbelt.Beam.IsActive.Changed += OnBeamChanged;
            _toolbelt.Beam.IsLocked.Changed += OnBeamLockChanged;
            _calibration.Progress.Changed += OnCalibrationChanged;
            _pause.Reasons.Changed += OnPauseChanged;
            _wasCalibrated = _calibration.IsComplete;
        }

        void ITickable.Tick()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (_ghost == null || _pause.IsPaused)
                return;

            _whisper?.SetPosition(_ghost.Position);
            _beam?.SetPitch(Mathf.Lerp(_audio.BeamPitchMin, _audio.BeamPitchMax, _toolbelt.Beam.Progress.Value));

            if (_ghost.IsBeamed)
            {
                _timeUntilBeamPulse -= deltaTime;
                if (_timeUntilBeamPulse <= 0f)
                {
                    // The last surge fights back in the hand too: harder, faster pulses until it breaks or is caught.
                    var surging = _ghost.IsSurging;
                    _haptics.Play(surging ? HapticStrength.Medium : HapticStrength.Light);
                    _timeUntilBeamPulse = surging ? _audio.SurgeHapticInterval : _audio.BeamHapticInterval;
                }
            }

            // The moment the lens first pulls the ghost out of hiding gets its own flash, so a reveal is never missed.
            var revealed = _ghost.VisibleReveal >= _vfxConfig.RevealPulseThreshold;
            if (revealed && !_wasRevealed && !_ghost.IsCaptured && !_ghost.IsEscaped)
            {
                _vfx.Play(VfxId.RevealPulse, _ghost.Position, RimColor);
                _sfx.Play3D(_audio.GhostReveal, _ghost.Position, _audio.RevealVolume);
            }

            _wasRevealed = revealed;

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
            _toolbelt.Beam.IsLocked.Changed -= OnBeamLockChanged;
            _calibration.Progress.Changed -= OnCalibrationChanged;
            _pause.Reasons.Changed -= OnPauseChanged;
            DetachGhost();
            StopBeam();
        }

        private void OnGhostChanged(Ghost ghost)
        {
            DetachGhost();
            _ghost = ghost;
            _wasCaptured = false;
            _wasEscaped = false;
            _wasRevealed = false;
            if (_ghost == null)
                return;

            _ghost.Teleported += OnTeleported;
            _ghost.Dashed += OnDashed;
            _ghost.Shrieked += OnShrieked;
            _ghost.Staggered += OnStaggered;
            _ghost.SurgeStarted += OnSurgeStarted;
            _ghost.Crept += OnCrept;
            _ghost.Lunged += OnLunged;
            _ghost.Settled += OnSettled;
            var clip = _session.GhostData != null ? _session.GhostData.WhisperClip : null;
            _whisper = _sfx.PlayLoop(clip, _audio.WhisperVolume, true);
            _whisper?.SetPosition(_ghost.Position);
        }

        private void DetachGhost()
        {
            if (_ghost != null)
            {
                _ghost.Teleported -= OnTeleported;
                _ghost.Dashed -= OnDashed;
                _ghost.Shrieked -= OnShrieked;
                _ghost.Staggered -= OnStaggered;
                _ghost.SurgeStarted -= OnSurgeStarted;
                _ghost.Crept -= OnCrept;
                _ghost.Lunged -= OnLunged;
                _ghost.Settled -= OnSettled;
            }

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
            PlayVoice(Voice?.Capture, _audio.CaptureCryVolume);
            _haptics.Play(HapticStrength.Heavy);
            _vfx.Play(VfxId.CaptureSpiral, _ghost.Position, RimColor);
        }

        private void OnEscapeStarted()
        {
            _whisper?.Stop();
            _whisper = null;
            _sfx.Play3D(Or(Voice?.Escape, _audio.GhostEscape), _ghost.Position, _audio.EscapeVolume);
        }

        private void OnTeleported(Vector3 from, Vector3 to)
        {
            _sfx.Play3D(Or(Voice?.Ability, _audio.TeleportWhoosh), to, _audio.TeleportVolume);
            _vfx.Play(VfxId.TeleportFlash, from, RimColor);
            _vfx.Play(VfxId.TeleportFlash, to, RimColor);
        }

        private void OnDashed(Vector3 from, Vector3 to)
        {
            _sfx.Play3D(Or(Voice?.Ability, _audio.DashWhoosh), to, _audio.DashVolume);
            _vfx.Play(VfxId.DashStreak, from, RimColor);
        }

        private void OnShrieked()
        {
            _sfx.Play3D(Or(Voice?.Ability, _audio.Shriek), _ghost.Position, _audio.ShriekVolume);
            _vfx.Play(VfxId.ShriekWave, _ghost.Position, RimColor);
            _haptics.Play(HapticStrength.Heavy);
        }

        private void OnStaggered()
        {
            // The freeze on being noticed coincides with the reveal pulse; that moment already has its own flash. A cat
            // sitting down is not knocked out either: it meows instead of sparking.
            if (_ghost.IsAlerted || _ghost.IsSettled)
                return;

            _sfx.Play3D(_audio.GhostStagger, _ghost.Position, _audio.StaggerVolume);
            PlayVoice(Voice?.Stagger, _audio.StaggerGruntVolume);
            _vfx.Play(VfxId.StaggerSparks, _ghost.Position, RimColor);
            _haptics.Play(HapticStrength.Medium);
        }

        private void OnCrept()
        {
            _sfx.Play3D(Or(Voice?.Ability, _audio.LurkerCreak), _ghost.Position, _audio.CreakVolume);
        }

        // Behind the player the face-rush of a jump scare would go unseen, so the lunge is all sound and hand.
        private void OnLunged()
        {
            if (_settings.JumpScares.Value)
            {
                _sfx.Play2D(_audio.ScareSting, _audio.ScareVolume, 1f);
                _haptics.Play(HapticStrength.Heavy);
                return;
            }

            var whisper = _session.GhostData != null ? _session.GhostData.WhisperClip : null;
            _sfx.Play2D(whisper, _audio.LungeWhisperVolume, 1f);
            _haptics.Play(HapticStrength.Medium);
        }

        private void OnSettled()
        {
            _sfx.Play3D(Or(Voice?.Ability, _audio.CatMeow), _ghost.Position, _audio.MeowVolume);
            _sfx.Play3D(_audio.CatPurr, _ghost.Position, _audio.PurrVolume);
            _haptics.Play(HapticStrength.Light);
        }

        private void OnSurgeStarted()
        {
            _sfx.Play2D(_audio.GhostSurge, _audio.SurgeVolume, 1f);
            _haptics.Play(HapticStrength.Heavy);
        }

        private void OnScared()
        {
            _sfx.Play2D(_audio.ScareSting, _audio.ScareVolume, 1f);
            var scream = Voice?.Scare;
            if (scream != null)
                _sfx.Play2D(scream, _audio.ScreamVolume, 1f);
            _haptics.Play(HapticStrength.Heavy);
        }

        // The ghost's own cry on top of the shared effect, from where it is.
        private void PlayVoice(AudioClip clip, float volume)
        {
            if (clip != null)
                _sfx.Play3D(clip, _ghost.Position, volume);
        }

        // Unity's null check, not ??: an empty clip slot deserializes as a destroyed-looking object, not a plain null.
        private static AudioClip Or(AudioClip clip, AudioClip fallback)
        {
            return clip != null ? clip : fallback;
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

        private void OnBeamLockChanged(bool locked)
        {
            if (!locked)
                return;

            _sfx.Play2D(_audio.BeamLock, _audio.BeamLockVolume, 1f);
            _haptics.Play(HapticStrength.Medium);
        }

        private void StopBeam()
        {
            _beam?.Stop();
            _beam = null;
        }

        private void OnPauseChanged(PauseReason reasons)
        {
            _whisper?.SetPaused(_pause.IsPaused);
            _beam?.SetPaused(_pause.IsPaused);
        }

        private void OnCalibrationChanged(float progress)
        {
            if (_calibration.IsComplete && !_wasCalibrated)
                _sfx.Play2D(_audio.ScanComplete, _audio.ScanVolume, 1f);
            _wasCalibrated = _calibration.IsComplete;
        }
    }
}
