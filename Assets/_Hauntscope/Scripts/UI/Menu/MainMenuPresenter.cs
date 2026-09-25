using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class MainMenuPresenter : IStartable, ITickable, IDisposable
    {
        private const string VersionKey = "menu.version";
        private const string TimestampKey = "menu.timestamp";

        private readonly MainMenuView _view;
        private readonly MenuNavigation _navigation;
        private readonly PlayerProgress _progress;
        private readonly HuntLauncher _launcher;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private int _shownSecond = -1;

        public MainMenuPresenter(
            MainMenuView view,
            MenuNavigation navigation,
            PlayerProgress progress,
            HuntLauncher launcher,
            ILocalizationService localization,
            UiFeedback ui)
        {
            _view = view;
            _navigation = navigation;
            _progress = progress;
            _launcher = launcher;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _navigation.Current.Changed += OnScreenChanged;
            _launcher.Prompt.Changed += OnPromptChanged;
            _progress.Ectoplasm.Changed += OnEctoplasmChanged;
            _localization.Changed += OnLanguageChanged;
            _view.StartClicked += OnStartClicked;
            _view.BestiaryClicked += OnBestiaryClicked;
            _view.SettingsClicked += OnSettingsClicked;

            RefreshVisibility();
            OnEctoplasmChanged(_progress.Ectoplasm.Value);
            OnLanguageChanged();
        }

        // A camcorder date stamp in the corner of the menu, ticking with the real clock.
        public void Tick()
        {
            var now = DateTime.Now;
            if (now.Second == _shownSecond)
                return;

            _shownSecond = now.Second;
            _view.SetTimestamp(_localization.Get(LocalizationTable.Ui, TimestampKey, now));
        }

        public void Dispose()
        {
            _navigation.Current.Changed -= OnScreenChanged;
            _launcher.Prompt.Changed -= OnPromptChanged;
            _progress.Ectoplasm.Changed -= OnEctoplasmChanged;
            _localization.Changed -= OnLanguageChanged;
            _view.StartClicked -= OnStartClicked;
            _view.BestiaryClicked -= OnBestiaryClicked;
            _view.SettingsClicked -= OnSettingsClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            RefreshVisibility();
        }

        private void OnPromptChanged(LaunchPrompt prompt)
        {
            RefreshVisibility();
        }

        // Launch prompts replace the main screen like any other menu screen, instead of stacking over it.
        private void RefreshVisibility()
        {
            _view.SetVisible(_navigation.Current.Value == MenuScreen.Main && _launcher.Prompt.Value == LaunchPrompt.None);
        }

        private void OnEctoplasmChanged(int amount)
        {
            _view.SetEctoplasm(amount);
        }

        private void OnLanguageChanged()
        {
            _shownSecond = -1;
            _view.SetVersion(_localization.Get(LocalizationTable.Ui, VersionKey, Application.version));
        }

        private void OnStartClicked()
        {
            _ui.PlayClick();
            _launcher.LaunchAsync(_lifetime.Token).Forget();
        }

        private void OnBestiaryClicked()
        {
            _ui.PlayClick();
            _navigation.Show(MenuScreen.Bestiary);
        }

        private void OnSettingsClicked()
        {
            _ui.PlayClick();
            _navigation.Show(MenuScreen.Settings);
        }
    }
}
