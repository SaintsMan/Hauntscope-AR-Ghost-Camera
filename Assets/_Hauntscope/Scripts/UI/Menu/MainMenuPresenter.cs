using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Ghosts;
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
        private const string ArHintKey = "menu.mode.ar_hint";
        private const string VirtualHintKey = "menu.mode.virtual_hint";
        private const string ArUnavailableKey = "menu.mode.ar_unavailable";
        private const string StandbyKey = "splash.standby";
        private const string WitchingHourKey = "menu.witching_hour";
        private const string DayChannelKey = "menu.channel.day";
        private const string NightChannelKey = "menu.channel.night";

        private readonly MainMenuView _view;
        private readonly MenuNavigation _navigation;
        private readonly PlayerProgress _progress;
        private readonly HuntLauncher _launcher;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly GameSettings _settings;
        private readonly SettingsRepository _settingsRepository;
        private readonly IArAvailability _arAvailability;
        private readonly WitchingHour _witchingHour;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private int _shownSecond = -1;
        private bool? _shownWitchingHour;
        private bool _arAvailable = true;

        public MainMenuPresenter(
            MainMenuView view,
            MenuNavigation navigation,
            PlayerProgress progress,
            HuntLauncher launcher,
            ILocalizationService localization,
            UiFeedback ui,
            GameSettings settings,
            SettingsRepository settingsRepository,
            IArAvailability arAvailability,
            WitchingHour witchingHour)
        {
            _witchingHour = witchingHour;
            _settings = settings;
            _settingsRepository = settingsRepository;
            _arAvailability = arAvailability;
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
            _view.ShopClicked += OnShopClicked;
            _view.ArModeClicked += OnArModeClicked;
            _view.VirtualModeClicked += OnVirtualModeClicked;
            _settings.Environment.Changed += OnEnvironmentChanged;

            RefreshVisibility();
            RenderMode();
            CheckArAsync(_lifetime.Token).Forget();
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
            if (_witchingHour.IsActive != _shownWitchingHour)
                RenderStatus();
        }

        private void RenderStatus()
        {
            var night = _witchingHour.IsActive;
            _shownWitchingHour = night;
            _view.SetStatus(_localization.Get(LocalizationTable.Ui, night ? WitchingHourKey : StandbyKey),
                _localization.Get(LocalizationTable.Ui, night ? NightChannelKey : DayChannelKey), night);
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
            _view.ShopClicked -= OnShopClicked;
            _view.ArModeClicked -= OnArModeClicked;
            _view.VirtualModeClicked -= OnVirtualModeClicked;
            _settings.Environment.Changed -= OnEnvironmentChanged;
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
            RenderMode();
            RenderStatus();
        }

        // A device without ARCore support can only hunt in the Virtual Room; the camera half stays visible but disabled,
        // so the player learns why instead of wondering where the option went.
        private async UniTaskVoid CheckArAsync(CancellationToken cancellationToken)
        {
            var availability = await _arAvailability.CheckAsync(cancellationToken);
            _arAvailable = availability != ArAvailabilityResult.Unsupported;
            _view.SetArAvailable(_arAvailable);
            RenderMode();
        }

        private void OnArModeClicked()
        {
            SelectEnvironment(HuntEnvironment.Ar);
        }

        private void OnVirtualModeClicked()
        {
            SelectEnvironment(HuntEnvironment.Virtual);
        }

        private void SelectEnvironment(HuntEnvironment environment)
        {
            if (!_arAvailable || _settings.Environment.Value == environment)
                return;

            _ui.PlayClick();
            _settings.SetEnvironment(environment);
            _settingsRepository.Save(_settings);
        }

        private void OnEnvironmentChanged(HuntEnvironment environment)
        {
            RenderMode();
        }

        private void RenderMode()
        {
            var isVirtual = !_arAvailable || _settings.Environment.Value == HuntEnvironment.Virtual;
            _view.SetMode(isVirtual);
            var key = !_arAvailable ? ArUnavailableKey : isVirtual ? VirtualHintKey : ArHintKey;
            _view.SetModeCaption(_localization.Get(LocalizationTable.Ui, key));
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

        private void OnShopClicked()
        {
            _ui.PlayClick();
            _navigation.ShowShop(ShopTab.Lasers);
        }

        private void OnSettingsClicked()
        {
            _ui.PlayClick();
            _navigation.Show(MenuScreen.Settings);
        }
    }
}
