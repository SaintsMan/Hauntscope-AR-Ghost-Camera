using System;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class CreditsPresenter : IStartable, IDisposable
    {
        private readonly CreditsView _view;
        private readonly MenuNavigation _navigation;

        public CreditsPresenter(CreditsView view, MenuNavigation navigation)
        {
            _view = view;
            _navigation = navigation;
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
            _navigation.Back();
        }
    }
}
