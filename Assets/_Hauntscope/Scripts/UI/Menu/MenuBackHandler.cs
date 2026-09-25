using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // The only listener of Android Back in the menu: screen presenters subscribing on their own would all react to
    // the same press (Credits -> Settings -> Main in one tap). Order follows what is on top: launch prompt, then the
    // menu screen stack, then the app itself.
    public sealed class MenuBackHandler : IStartable, IDisposable
    {
        private readonly IBackButton _backButton;
        private readonly MenuNavigation _navigation;
        private readonly HuntLauncher _launcher;
        private readonly ISystemNavigation _system;
        private readonly UiFeedback _ui;

        public MenuBackHandler(
            IBackButton backButton,
            MenuNavigation navigation,
            HuntLauncher launcher,
            ISystemNavigation system,
            UiFeedback ui)
        {
            _backButton = backButton;
            _navigation = navigation;
            _launcher = launcher;
            _system = system;
            _ui = ui;
        }

        public void Start()
        {
            _backButton.Pressed += OnBackPressed;
        }

        public void Dispose()
        {
            _backButton.Pressed -= OnBackPressed;
        }

        private void OnBackPressed()
        {
            if (_launcher.CancelPrompt())
            {
                _ui.PlayBack();
                return;
            }

            if (_navigation.Current.Value != MenuScreen.Main)
            {
                _ui.PlayBack();
                _navigation.Back();
                return;
            }

            _system.MoveToBackground();
        }
    }
}
