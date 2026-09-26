using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class HuntHudPresenter : IStartable, ITickable, IDisposable
    {
        private const string EmfLevelKey = "hud.emf.level";
        private const string FocusKey = "hud.focus";
        private const float FocusStep = 10f;

        private readonly HuntHudView _view;
        private readonly EmfRadar _radar;
        private readonly Toolbelt _toolbelt;
        private readonly RoomCalibration _calibration;
        private readonly ILocalizationService _localization;
        private readonly HuntSession _session;
        private readonly ScareConfig _scareConfig;
        private readonly HudConfig _hudConfig;
        private readonly HuntPause _pause;
        private readonly HuntModifiers _modifiers;
        private readonly HuntLoadout _loadout;
        private readonly ToolsConfig _toolsConfig;
        private readonly CaptureConfig _captureConfig;

        private float _scareFlash;
        private float _shownProgress;
        private bool _isShaking;
        private bool _isFocusShown;
        private int _shownFocusTenths = -1;
        private bool _isVulnerableShown;
        private bool _isSurgeShown;

        public HuntHudPresenter(
            HuntHudView view,
            EmfRadar radar,
            Toolbelt toolbelt,
            RoomCalibration calibration,
            ILocalizationService localization,
            HuntSession session,
            ScareConfig scareConfig,
            HudConfig hudConfig,
            HuntPause pause,
            HuntModifiers modifiers,
            HuntLoadout loadout,
            ToolsConfig toolsConfig,
            CaptureConfig captureConfig)
        {
            _toolsConfig = toolsConfig;
            _captureConfig = captureConfig;
            _modifiers = modifiers;
            _loadout = loadout;
            _hudConfig = hudConfig;
            _pause = pause;
            _view = view;
            _radar = radar;
            _toolbelt = toolbelt;
            _calibration = calibration;
            _localization = localization;
            _session = session;
            _scareConfig = scareConfig;
        }

        public void Start()
        {
            _radar.Level.Changed += OnEmfLevelChanged;
            _toolbelt.Lens.IsActive.Changed += OnLensActiveChanged;
            _toolbelt.Beam.IsActive.Changed += OnBeamActiveChanged;
            _toolbelt.Beam.Progress.Changed += OnCaptureProgressChanged;
            _toolbelt.Beam.IsLocked.Changed += OnBeamLockChanged;
            _calibration.Progress.Changed += OnCalibrationChanged;
            _session.Result.Changed += OnResultChanged;
            _session.Scared += OnScared;
            _localization.Changed += OnLanguageChanged;
            _view.LensClicked += OnLensClicked;
            _view.BeamPressed += OnBeamPressed;
            _view.BeamReleased += OnBeamReleased;
            _loadout.Changed += OnLoadoutChanged;

            OnLoadoutChanged();
            UpdateVisibility();
            _view.SetScareFlash(0f);
            _view.SetFocusVisible(false);
            _view.SetVulnerable(false);
            _view.SetLensActive(_toolbelt.Lens.IsActive.Value);
            _view.SetBeamActive(_toolbelt.Beam.IsActive.Value);
            _view.SetCaptureProgress(_toolbelt.Beam.Progress.Value);
            RenderEmf(_radar.Level.Value);
        }

        public void Tick()
        {
            var ghost = _session.Ghost.Value;
            var paused = _pause.IsPaused;
            var surging = ghost != null && ghost.IsSurging && !paused;
            var shaking = (_toolbelt.Beam.IsLocked.Value || surging) && !paused;
            if (shaking || _isShaking)
                _view.SetReticleShake(!shaking ? 0f : surging ? _hudConfig.SurgeShake : _toolbelt.Beam.Progress.Value * _hudConfig.ReticleShake);
            _isShaking = shaking;
            if (surging != _isSurgeShown)
            {
                _isSurgeShown = surging;
                _view.SetSurging(surging);
            }

            RenderFocus(ghost, paused);
            _view.SetEmfBearing(_modifiers.ShowsEmfDirection && _radar.Level.Value > 0 && !_pause.IsPaused, _radar.Bearing);

            if (_scareFlash <= 0f)
                return;

            _scareFlash = Mathf.Max(0f, _scareFlash - Time.deltaTime / _scareConfig.Duration);
            _view.SetScareFlash(_scareFlash);
        }

        public void Dispose()
        {
            _radar.Level.Changed -= OnEmfLevelChanged;
            _toolbelt.Lens.IsActive.Changed -= OnLensActiveChanged;
            _toolbelt.Beam.IsActive.Changed -= OnBeamActiveChanged;
            _toolbelt.Beam.Progress.Changed -= OnCaptureProgressChanged;
            _toolbelt.Beam.IsLocked.Changed -= OnBeamLockChanged;
            _calibration.Progress.Changed -= OnCalibrationChanged;
            _session.Result.Changed -= OnResultChanged;
            _session.Scared -= OnScared;
            _localization.Changed -= OnLanguageChanged;
            _view.LensClicked -= OnLensClicked;
            _view.BeamPressed -= OnBeamPressed;
            _view.BeamReleased -= OnBeamReleased;
            _loadout.Changed -= OnLoadoutChanged;
        }

        // The equipped laser (and any future booster) sets how wide the capture ring is.
        private void OnLoadoutChanged()
        {
            _view.SetReticleRadius(_toolbelt.Beam.ReticleRadius);
        }

        private void OnScared()
        {
            _scareFlash = 1f;
            _view.SetScareFlash(_scareFlash);
        }

        private void OnLensClicked()
        {
            _toolbelt.ToggleLens();
        }

        private void OnBeamPressed()
        {
            _toolbelt.StartBeam();
        }

        private void OnBeamReleased()
        {
            _toolbelt.StopBeam();
        }

        private void OnLensActiveChanged(bool active)
        {
            _view.SetLensActive(active);
        }

        private void OnBeamActiveChanged(bool active)
        {
            _view.SetBeamActive(active);
        }

        private void OnCaptureProgressChanged(float progress)
        {
            _view.SetCaptureProgress(progress);
            if (progress >= 1f && _shownProgress < 1f)
                _view.PlayCaptureFlash();
            _shownProgress = progress;
        }

        private void OnBeamLockChanged(bool locked)
        {
            _view.SetBeamLocked(locked);
        }

        private void OnEmfLevelChanged(int level)
        {
            RenderEmf(level);
        }

        private void OnCalibrationChanged(float progress)
        {
            UpdateVisibility();
        }

        private void OnResultChanged(HuntResult result)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            _view.SetVisible(_calibration.IsComplete && _session.Result.Value == null);
        }

        private void OnLanguageChanged()
        {
            RenderEmf(_radar.Level.Value);
            _shownFocusTenths = -1;
        }

        // The camcorder's focus scale doubles as the proximity gauge; the text is rebuilt only when the reading
        // changes by a tenth of a metre, so no string is formatted on a quiet frame.
        private void RenderFocus(Ghost ghost, bool paused)
        {
            var visible = ghost != null && !paused && !ghost.IsCaptured && !ghost.IsEscaped
                && ghost.VisibleReveal >= _toolsConfig.BeamRevealThreshold;
            if (visible != _isFocusShown)
            {
                _isFocusShown = visible;
                _view.SetFocusVisible(visible);
            }

            var vulnerable = visible && ghost.IsStaggered;
            if (vulnerable != _isVulnerableShown)
            {
                _isVulnerableShown = vulnerable;
                _view.SetVulnerable(vulnerable);
            }

            if (!visible)
                return;

            var distance = _toolbelt.Beam.GhostDistance;
            var tenths = Mathf.RoundToInt(distance * FocusStep);
            if (tenths == _shownFocusTenths)
                return;

            _shownFocusTenths = tenths;
            var zone = distance <= _scareConfig.Distance ? FocusZone.Danger
                : distance <= _captureConfig.FarDistance ? FocusZone.Good
                : FocusZone.Far;
            _view.SetFocus(_localization.Get(LocalizationTable.Ui, FocusKey, tenths / FocusStep), zone);
        }

        private void RenderEmf(int level)
        {
            _view.SetEmfLevel(level);
            _view.SetEmfLevelText(_localization.Get(LocalizationTable.Ui, EmfLevelKey, level));
        }
    }
}
