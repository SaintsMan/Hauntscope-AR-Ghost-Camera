using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class SettingsPresenter : IStartable, IDisposable
    {
        private const string OnKey = "common.on";
        private const string OffKey = "common.off";
        private const string LanguageNameKeyPrefix = "settings.language_name.";

        private readonly SettingsView _view;
        private readonly MenuNavigation _navigation;
        private readonly GameSettings _settings;
        private readonly SettingsRepository _repository;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly IAdPrivacy _privacy;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public SettingsPresenter(
            SettingsView view,
            MenuNavigation navigation,
            GameSettings settings,
            SettingsRepository repository,
            ILocalizationService localization,
            UiFeedback ui,
            IAdPrivacy privacy)
        {
            _privacy = privacy;
            _view = view;
            _navigation = navigation;
            _settings = settings;
            _repository = repository;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _navigation.Current.Changed += OnScreenChanged;
            _localization.Changed += Render;
            _view.BackClicked += OnBackClicked;
            _view.CreditsClicked += OnCreditsClicked;
            _view.SoundClicked += OnSoundClicked;
            _view.VibrationClicked += OnVibrationClicked;
            _view.JumpScaresClicked += OnJumpScaresClicked;
            _view.LanguageClicked += OnLanguageClicked;
            _view.PrivacyClicked += OnPrivacyClicked;

            OnScreenChanged(_navigation.Current.Value);
            Render();
        }

        public void Dispose()
        {
            _navigation.Current.Changed -= OnScreenChanged;
            _localization.Changed -= Render;
            _view.BackClicked -= OnBackClicked;
            _view.CreditsClicked -= OnCreditsClicked;
            _view.SoundClicked -= OnSoundClicked;
            _view.VibrationClicked -= OnVibrationClicked;
            _view.JumpScaresClicked -= OnJumpScaresClicked;
            _view.LanguageClicked -= OnLanguageClicked;
            _view.PrivacyClicked -= OnPrivacyClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            _view.SetVisible(screen == MenuScreen.Settings);
            // Consent status arrives after start-up, so it is checked each time the screen opens.
            if (screen == MenuScreen.Settings)
                _view.SetPrivacyVisible(_privacy.IsOptionsRequired);
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

        private void OnLanguageClicked()
        {
            var languages = _localization.AvailableLanguages;
            if (languages.Count == 0)
                return;

            var current = 0;
            for (var i = 0; i < languages.Count; i++)
            {
                if (languages[i] == _localization.CurrentLanguage)
                    current = i;
            }

            _settings.SetLanguage(languages[(current + 1) % languages.Count]);
            SaveAndRender();
        }

        private void OnCreditsClicked()
        {
            _ui.PlayClick();
            _navigation.Show(MenuScreen.Credits);
        }

        private void OnBackClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }

        private void SaveAndRender()
        {
            _ui.PlayClick();
            _repository.Save(_settings);
            Render();
        }

        private void Render()
        {
            _view.SetSound(_settings.Sound.Value, State(_settings.Sound.Value));
            _view.SetVibration(_settings.Vibration.Value, State(_settings.Vibration.Value));
            _view.SetJumpScares(_settings.JumpScares.Value, State(_settings.JumpScares.Value));
            _view.SetLanguage(_localization.Get(LocalizationTable.Ui, LanguageNameKeyPrefix + _localization.CurrentLanguage));
        }

        private string State(bool on)
        {
            return _localization.Get(LocalizationTable.Ui, on ? OnKey : OffKey);
        }

        private void OnPrivacyClicked()
        {
            _ui.PlayClick();
            _privacy.ShowOptionsAsync(_lifetime.Token).Forget();
        }
    }
}
