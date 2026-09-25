using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
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
        private readonly ToolsConfig _toolsConfig;
        private readonly HuntSession _session;

        public HuntHudPresenter(
            HuntHudView view,
            EmfRadar radar,
            Toolbelt toolbelt,
            RoomCalibration calibration,
            ILocalizationService localization,
            ToolsConfig toolsConfig,
            HuntSession session)
        {
            _view = view;
            _radar = radar;
            _toolbelt = toolbelt;
            _calibration = calibration;
            _localization = localization;
            _toolsConfig = toolsConfig;
            _session = session;
        }

        public void Start()
        {
            _radar.Level.Changed += OnEmfLevelChanged;
            _toolbelt.Lens.IsActive.Changed += OnLensActiveChanged;
            _toolbelt.Beam.IsActive.Changed += OnBeamActiveChanged;
            _toolbelt.Beam.Progress.Changed += OnCaptureProgressChanged;
            _calibration.Progress.Changed += OnCalibrationChanged;
            _session.Result.Changed += OnResultChanged;
            _localization.Changed += OnLanguageChanged;
            _view.LensClicked += OnLensClicked;
            _view.BeamPressed += OnBeamPressed;
            _view.BeamReleased += OnBeamReleased;

            _view.SetReticleRadius(_toolsConfig.ReticleRadius);
            UpdateVisibility();
            _view.SetLensActive(_toolbelt.Lens.IsActive.Value);
            _view.SetBeamActive(_toolbelt.Beam.IsActive.Value);
            _view.SetCaptureProgress(_toolbelt.Beam.Progress.Value);
            RenderEmf(_radar.Level.Value);
        }

        public void Dispose()
        {
            _radar.Level.Changed -= OnEmfLevelChanged;
            _toolbelt.Lens.IsActive.Changed -= OnLensActiveChanged;
            _toolbelt.Beam.IsActive.Changed -= OnBeamActiveChanged;
            _toolbelt.Beam.Progress.Changed -= OnCaptureProgressChanged;
            _calibration.Progress.Changed -= OnCalibrationChanged;
            _session.Result.Changed -= OnResultChanged;
            _localization.Changed -= OnLanguageChanged;
            _view.LensClicked -= OnLensClicked;
            _view.BeamPressed -= OnBeamPressed;
            _view.BeamReleased -= OnBeamReleased;
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
        }

        private void RenderEmf(int level)
        {
            _view.SetEmfLevel(level);
            _view.SetEmfLevelText(_localization.Get(LocalizationTable.Ui, EmfLevelKey, level));
        }
    }
}
