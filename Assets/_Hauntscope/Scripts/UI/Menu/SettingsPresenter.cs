using System;
using Hauntscope.Core.Services;
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

        public SettingsPresenter(
            SettingsView view,
            MenuNavigation navigation,
            GameSettings settings,
            SettingsRepository repository,
            ILocalizationService localization)
        {
            _view = view;
            _navigation = navigation;
            _settings = settings;
            _repository = repository;
            _localization = localization;
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
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            _view.SetVisible(screen == MenuScreen.Settings);
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
            _navigation.Show(MenuScreen.Credits);
        }

        private void OnBackClicked()
        {
            _navigation.Back();
        }

        private void SaveAndRender()
        {
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
    }
}
