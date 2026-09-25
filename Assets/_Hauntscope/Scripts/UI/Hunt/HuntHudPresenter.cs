using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class HuntHudPresenter : IStartable, IDisposable
    {
        private const string EmfLevelKey = "hud.emf.level";

        private readonly HuntHudView _view;
        private readonly EmfRadar _radar;
        private readonly RoomCalibration _calibration;
        private readonly ILocalizationService _localization;

        public HuntHudPresenter(
            HuntHudView view,
            EmfRadar radar,
            RoomCalibration calibration,
            ILocalizationService localization)
        {
            _view = view;
            _radar = radar;
            _calibration = calibration;
            _localization = localization;
        }

        public void Start()
        {
            _radar.Level.Changed += OnEmfLevelChanged;
            _calibration.Progress.Changed += OnCalibrationChanged;
            _localization.Changed += OnLanguageChanged;
            _view.SetVisible(_calibration.IsComplete);
            RenderEmf(_radar.Level.Value);
        }

        public void Dispose()
        {
            _radar.Level.Changed -= OnEmfLevelChanged;
            _calibration.Progress.Changed -= OnCalibrationChanged;
            _localization.Changed -= OnLanguageChanged;
        }

        private void OnEmfLevelChanged(int level)
        {
            RenderEmf(level);
        }

        private void OnCalibrationChanged(float progress)
        {
            _view.SetVisible(_calibration.IsComplete);
        }

        private void OnLanguageChanged()
        {
            RenderEmf(_radar.Level.Value);
        }

        private void RenderEmf(int level)
        {
            _view.SetEmfLevel(level);
            _view.SetEmfLevelText(_localization.Get(LocalizationTable.Ui, EmfLevelKey, level));
        }
    }
}
