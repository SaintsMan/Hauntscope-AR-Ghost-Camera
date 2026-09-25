using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Progress;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class MainMenuPresenter : IStartable, IDisposable
    {
        private const string VersionKey = "menu.version";

        private readonly MainMenuView _view;
        private readonly MenuNavigation _navigation;
        private readonly PlayerProgress _progress;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILocalizationService _localization;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public MainMenuPresenter(
            MainMenuView view,
            MenuNavigation navigation,
            PlayerProgress progress,
            ISceneLoader sceneLoader,
            ILocalizationService localization)
        {
            _view = view;
            _navigation = navigation;
            _progress = progress;
            _sceneLoader = sceneLoader;
            _localization = localization;
        }

        public void Start()
        {
            _navigation.Current.Changed += OnScreenChanged;
            _progress.Ectoplasm.Changed += OnEctoplasmChanged;
            _localization.Changed += OnLanguageChanged;
            _view.StartClicked += OnStartClicked;
            _view.BestiaryClicked += OnBestiaryClicked;
            _view.SettingsClicked += OnSettingsClicked;

            OnScreenChanged(_navigation.Current.Value);
            OnEctoplasmChanged(_progress.Ectoplasm.Value);
            OnLanguageChanged();
        }

        public void Dispose()
        {
            _navigation.Current.Changed -= OnScreenChanged;
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
            _view.SetVisible(screen == MenuScreen.Main);
        }

        private void OnEctoplasmChanged(int amount)
        {
            _view.SetEctoplasm(amount.ToString());
        }

        private void OnLanguageChanged()
        {
            _view.SetVersion(_localization.Get(LocalizationTable.Ui, VersionKey, Application.version));
        }

        private void OnStartClicked()
        {
            _sceneLoader.LoadAsync(SceneId.Hunt, _lifetime.Token).Forget();
        }

        private void OnBestiaryClicked()
        {
            _navigation.Show(MenuScreen.Bestiary);
        }

        private void OnSettingsClicked()
        {
            _navigation.Show(MenuScreen.Settings);
        }
    }
}
