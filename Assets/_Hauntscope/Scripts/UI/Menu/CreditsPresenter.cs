using System;
using Hauntscope.Gameplay.Feedback;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class CreditsPresenter : IStartable, IDisposable
    {
        private readonly CreditsView _view;
        private readonly MenuNavigation _navigation;
        private readonly UiFeedback _ui;

        public CreditsPresenter(CreditsView view, MenuNavigation navigation, UiFeedback ui)
        {
            _view = view;
            _navigation = navigation;
            _ui = ui;
        }

        public void Start()
        {
            _navigation.Current.Changed += OnScreenChanged;
            _view.BackClicked += OnBackClicked;
            OnScreenChanged(_navigation.Current.Value);
        }

        public void Dispose()
        {
            _navigation.Current.Changed -= OnScreenChanged;
            _view.BackClicked -= OnBackClicked;
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            _view.SetVisible(screen == MenuScreen.Credits);
        }

        private void OnBackClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }
    }
}
