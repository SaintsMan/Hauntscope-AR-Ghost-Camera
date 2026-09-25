using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class ScanHudPresenter : IStartable, ITickable, IDisposable
    {
        private const string ProgressKey = "hud.scan.progress";
        private const string CalibratedKey = "hud.scan.calibrated";
        private const string PointAtFloorKey = "hud.scan.hint_point_at_floor";
        private const string MoveSlowlyKey = "hud.scan.hint_move_slowly";
        private const float PercentMultiplier = 100f;

        private readonly ScanHudView _view;
        private readonly RoomCalibration _calibration;
        private readonly ILocalizationService _localization;
        private readonly RoomConfig _config;

        private int _shownPercent = -1;
        private string _shownHintKey;
        private float _hideCountdown;
        private bool _isHiding;

        public ScanHudPresenter(
            ScanHudView view,
            RoomCalibration calibration,
            ILocalizationService localization,
            RoomConfig config)
        {
            _view = view;
            _calibration = calibration;
            _localization = localization;
            _config = config;
        }

        public void Start()
        {
            _calibration.Progress.Changed += OnProgressChanged;
            _localization.Changed += OnLanguageChanged;
            Render(true);
        }

        public void Tick()
        {
            if (!_isHiding)
                return;

            _hideCountdown -= Time.deltaTime;
            if (_hideCountdown > 0f)
                return;

            _isHiding = false;
            _view.SetVisible(false);
        }

        public void Dispose()
        {
            _calibration.Progress.Changed -= OnProgressChanged;
            _localization.Changed -= OnLanguageChanged;
        }

        private void OnProgressChanged(float progress)
        {
            Render(false);
        }

        private void OnLanguageChanged()
        {
            Render(true);
        }

        private void Render(bool force)
        {
            var progress = _calibration.Progress.Value;
            _view.SetProgress(progress);
            // Like ARCore's own guide: the gesture is shown until the first floor plane turns up, after which the
            // planes themselves and the progress bar are the feedback.
            _view.SetCueVisible(progress <= 0f);
            UpdateVisibility();

            var percent = Mathf.FloorToInt(progress * PercentMultiplier);
            var hintKey = _calibration.IsComplete ? null : progress > 0f ? MoveSlowlyKey : PointAtFloorKey;
            if (!force && percent == _shownPercent && hintKey == _shownHintKey)
                return;

            _shownPercent = percent;
            _shownHintKey = hintKey;
            _view.SetStatus(_calibration.IsComplete
                ? _localization.Get(LocalizationTable.Ui, CalibratedKey)
                : _localization.Get(LocalizationTable.Ui, ProgressKey, percent));
            _view.SetHint(hintKey != null ? _localization.Get(LocalizationTable.Ui, hintKey) : string.Empty);
        }

        private void UpdateVisibility()
        {
            if (!_calibration.IsComplete)
            {
                _isHiding = false;
                _view.SetVisible(true);
                return;
            }

            if (_isHiding)
                return;

            _isHiding = true;
            _hideCountdown = _config.CalibratedMessageDuration;
        }
    }
}
