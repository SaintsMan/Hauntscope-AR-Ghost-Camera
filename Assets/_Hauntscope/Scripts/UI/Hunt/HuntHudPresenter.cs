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
        private readonly Toolbelt _toolbelt;
        private readonly RoomCalibration _calibration;
        private readonly ILocalizationService _localization;

        public HuntHudPresenter(
            HuntHudView view,
            EmfRadar radar,
            Toolbelt toolbelt,
            RoomCalibration calibration,
            ILocalizationService localization)
        {
            _view = view;
            _radar = radar;
            _toolbelt = toolbelt;
            _calibration = calibration;
            _localization = localization;
        }

        public void Start()
        {
            _radar.Level.Changed += OnEmfLevelChanged;
            _toolbelt.Lens.IsActive.Changed += OnLensActiveChanged;
            _calibration.Progress.Changed += OnCalibrationChanged;
            _localization.Changed += OnLanguageChanged;
            _view.LensClicked += OnLensClicked;

            _view.SetVisible(_calibration.IsComplete);
            _view.SetLensActive(_toolbelt.Lens.IsActive.Value);
            RenderEmf(_radar.Level.Value);
        }

        public void Dispose()
        {
            _radar.Level.Changed -= OnEmfLevelChanged;
            _toolbelt.Lens.IsActive.Changed -= OnLensActiveChanged;
            _calibration.Progress.Changed -= OnCalibrationChanged;
            _localization.Changed -= OnLanguageChanged;
            _view.LensClicked -= OnLensClicked;
        }

        private void OnLensClicked()
        {
            _toolbelt.ToggleLens();
        }

        private void OnLensActiveChanged(bool active)
        {
            _view.SetLensActive(active);
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
