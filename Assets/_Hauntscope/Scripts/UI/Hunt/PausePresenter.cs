using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class PausePresenter : IStartable, IDisposable
    {
        private const string OnKey = "common.on";
        private const string OffKey = "common.off";

        private readonly PauseView _view;
        private readonly HuntPause _pause;
        private readonly GameSettings _settings;
        private readonly SettingsRepository _repository;
        private readonly IOcclusionService _occlusion;
        private readonly ILocalizationService _localization;
        private readonly ISceneLoader _sceneLoader;
        private readonly IBackButton _backButton;
        private readonly HuntSession _session;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private bool _settingsShown;

        public PausePresenter(
            PauseView view,
            HuntPause pause,
            GameSettings settings,
            SettingsRepository repository,
            IOcclusionService occlusion,
            ILocalizationService localization,
            ISceneLoader sceneLoader,
            IBackButton backButton,
            HuntSession session,
            UiFeedback ui)
        {
            _view = view;
            _pause = pause;
            _settings = settings;
            _repository = repository;
            _occlusion = occlusion;
            _localization = localization;
            _sceneLoader = sceneLoader;
            _backButton = backButton;
            _session = session;
            _ui = ui;
        }

        public void Start()
        {
            _pause.Reasons.Changed += OnPauseChanged;
            _localization.Changed += RenderSettings;
            _view.ResumeClicked += OnResumeClicked;
            _view.SettingsClicked += OnSettingsClicked;
            _view.QuitClicked += OnQuitClicked;
            _view.BackClicked += OnBackClicked;
            _view.SoundClicked += OnSoundClicked;
            _view.VibrationClicked += OnVibrationClicked;
            _view.JumpScaresClicked += OnJumpScaresClicked;
            _view.OcclusionClicked += OnOcclusionClicked;
            _backButton.Pressed += OnBackPressed;

            ShowSettings(false);
            OnPauseChanged(_pause.Reasons.Value);
        }

        public void Dispose()
        {
            _pause.Reasons.Changed -= OnPauseChanged;
            _localization.Changed -= RenderSettings;
            _view.ResumeClicked -= OnResumeClicked;
            _view.SettingsClicked -= OnSettingsClicked;
            _view.QuitClicked -= OnQuitClicked;
            _view.BackClicked -= OnBackClicked;
            _view.SoundClicked -= OnSoundClicked;
            _view.VibrationClicked -= OnVibrationClicked;
            _view.JumpScaresClicked -= OnJumpScaresClicked;
            _view.OcclusionClicked -= OnOcclusionClicked;
            _backButton.Pressed -= OnBackPressed;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnPauseChanged(PauseReason reasons)
        {
            var visible = _pause.IsMenuRequested;
            if (!visible)
                ShowSettings(false);
            _view.SetVisible(visible);
        }

        private void OnResumeClicked()
        {
            _ui.PlayClick();
            _pause.Resume();
        }

        private void OnSettingsClicked()
        {
            _ui.PlayClick();
            _view.SetOcclusionAvailable(_occlusion.IsSupported);
            RenderSettings();
            ShowSettings(true);
        }

        private void OnBackClicked()
        {
            _ui.PlayBack();
            ShowSettings(false);
        }

        // Android Back mirrors the on-screen buttons: settings -> pause menu -> hunt, and opens the menu mid-hunt.
        // The result screen has its own buttons and nothing to pause, so Back is ignored there.
        private void OnBackPressed()
        {
            if (_session.Result.Value != null)
                return;

            if (_settingsShown)
            {
                OnBackClicked();
                return;
            }

            if (_pause.IsMenuRequested)
                _ui.PlayBack();
            else
                _ui.PlayClick();
            _pause.TogglePause();
        }

        private void ShowSettings(bool settings)
        {
            _settingsShown = settings;
            _view.ShowSettings(settings);
        }

        private void OnQuitClicked()
        {
            _ui.PlayBack();
            _sceneLoader.LoadAsync(SceneId.MainMenu, _lifetime.Token).Forget();
        }

        private void OnSoundClicked()
        {
            _settings.SetSound(!_settings.Sound.Value);
            SaveAndRender();
        }

        private void OnVibrationClicked()
        {
            _settings.SetVibration(!_settings.Vibration.Value);
            SaveAndRender();
        }

        private void OnJumpScaresClicked()
        {
            _settings.SetJumpScares(!_settings.JumpScares.Value);
            SaveAndRender();
        }

        private void OnOcclusionClicked()
        {
            _settings.SetOcclusion(!_settings.Occlusion.Value);
            SaveAndRender();
        }

        private void SaveAndRender()
        {
            _ui.PlayClick();
            _repository.Save(_settings);
            RenderSettings();
        }

        private void RenderSettings()
        {
            _view.SetSound(_settings.Sound.Value, State(_settings.Sound.Value));
            _view.SetVibration(_settings.Vibration.Value, State(_settings.Vibration.Value));
            _view.SetJumpScares(_settings.JumpScares.Value, State(_settings.JumpScares.Value));
            _view.SetOcclusion(_settings.Occlusion.Value, State(_settings.Occlusion.Value));
        }

        private string State(bool on)
        {
            return _localization.Get(LocalizationTable.Ui, on ? OnKey : OffKey);
        }
    }
}
