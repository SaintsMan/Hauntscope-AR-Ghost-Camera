using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.UI.Common;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // The only listener of Android Back in the menu: screen presenters subscribing on their own would all react to
    // the same press (Credits -> Settings -> Main in one tap). Order follows what is on top: an open photo, the launch
    // prompt, then the menu screen stack, then the app itself.
    public sealed class MenuBackHandler : IStartable, IDisposable
    {
        private readonly IBackButton _backButton;
        private readonly MenuNavigation _navigation;
        private readonly PhotoViewer _viewer;
        private readonly HuntLauncher _launcher;
        private readonly ISystemNavigation _system;
        private readonly UiFeedback _ui;
        private readonly MenuPopupQueue _popups;

        public MenuBackHandler(
            IBackButton backButton,
            MenuNavigation navigation,
            PhotoViewer viewer,
            HuntLauncher launcher,
            ISystemNavigation system,
            UiFeedback ui,
            MenuPopupQueue popups)
        {
            _popups = popups;
            _backButton = backButton;
            _navigation = navigation;
            _viewer = viewer;
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
            if (_viewer.IsOpen)
            {
                _ui.PlayBack();
                _viewer.Close();
                return;
            }

            if (_launcher.CancelPrompt())
            {
                _ui.PlayBack();
                return;
            }

            if (_popups.CloseCurrent())
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
