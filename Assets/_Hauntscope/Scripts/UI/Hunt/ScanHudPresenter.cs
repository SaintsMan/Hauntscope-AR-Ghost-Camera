using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class ScanHudPresenter : IStartable, IDisposable
    {
        private const string ProgressKey = "hud.scan.progress";
        private const string CalibratedKey = "hud.scan.calibrated";
        private const string PointAtFloorKey = "hud.scan.hint_point_at_floor";
        private const string MoveSlowlyKey = "hud.scan.hint_move_slowly";
        private const float PercentMultiplier = 100f;

        private readonly ScanHudView _view;
        private readonly RoomCalibration _calibration;
        private readonly ILocalizationService _localization;

        private int _shownPercent = -1;
        private string _shownHintKey;

        public ScanHudPresenter(ScanHudView view, RoomCalibration calibration, ILocalizationService localization)
        {
            _view = view;
            _calibration = calibration;
            _localization = localization;
        }

        public void Start()
        {
            _calibration.Progress.Changed += OnProgressChanged;
            _localization.Changed += OnLanguageChanged;
            Render(true);
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
    }
}
