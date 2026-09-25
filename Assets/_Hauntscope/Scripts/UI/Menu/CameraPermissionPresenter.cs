using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class CameraPermissionPresenter : IStartable, IDisposable
    {
        private const string BodyKey = "permission.camera.body";
        private const string SettingsBodyKey = "permission.camera.body_settings";
        private const string AllowKey = "permission.camera.allow";
        private const string OpenSettingsKey = "permission.camera.open_settings";

        private readonly CameraPermissionView _view;
        private readonly HuntLauncher _launcher;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public CameraPermissionPresenter(
            CameraPermissionView view,
            HuntLauncher launcher,
            ILocalizationService localization,
            UiFeedback ui)
        {
            _view = view;
            _launcher = launcher;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _launcher.Prompt.Changed += OnPromptChanged;
            _localization.Changed += OnLanguageChanged;
            _view.AllowClicked += OnAllowClicked;
            _view.VirtualClicked += OnVirtualClicked;

            OnPromptChanged(_launcher.Prompt.Value);
        }

        public void Dispose()
        {
            _launcher.Prompt.Changed -= OnPromptChanged;
            _localization.Changed -= OnLanguageChanged;
            _view.AllowClicked -= OnAllowClicked;
            _view.VirtualClicked -= OnVirtualClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnPromptChanged(LaunchPrompt prompt)
        {
            _view.SetVisible(prompt == LaunchPrompt.CameraPermission || prompt == LaunchPrompt.CameraSettings);
            RefreshTexts();
        }

        private void OnLanguageChanged()
        {
            RefreshTexts();
        }

        private void RefreshTexts()
        {
            var openSettings = _launcher.Prompt.Value == LaunchPrompt.CameraSettings;
            _view.SetBody(_localization.Get(LocalizationTable.Ui, openSettings ? SettingsBodyKey : BodyKey));
            _view.SetAllowLabel(_localization.Get(LocalizationTable.Ui, openSettings ? OpenSettingsKey : AllowKey));
        }

        private void OnAllowClicked()
        {
            _ui.PlayClick();
            _launcher.AllowCameraAsync(_lifetime.Token).Forget();
        }

        private void OnVirtualClicked()
        {
            _ui.PlayClick();
            _launcher.PlayVirtualAsync(_lifetime.Token).Forget();
        }
    }
}
