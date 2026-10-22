using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Season;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // Shows the event's strip while the event runs and counts down its days. It listens to Remote Config too: the console
    // may switch the event on (or off) after the menu has drawn.
    public sealed class HalloweenBannerPresenter : IStartable, IDisposable
    {
        private const string TitleKey = "halloween.banner.title";
        private const string DaysKey = "halloween.banner.days";
        private const string LastDayKey = "halloween.banner.last_day";

        private readonly HalloweenBannerView _view;
        private readonly HalloweenCollection _collection;
        private readonly HalloweenAnalytics _analytics;
        private readonly MenuNavigation _navigation;
        private readonly IRemoteValues _remote;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;

        public HalloweenBannerPresenter(HalloweenBannerView view, HalloweenCollection collection, HalloweenAnalytics analytics,
            MenuNavigation navigation, IRemoteValues remote, ILocalizationService localization, UiFeedback ui)
        {
            _view = view;
            _collection = collection;
            _analytics = analytics;
            _navigation = navigation;
            _remote = remote;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _collection.Candy.Changed += OnCandyChanged;
            _localization.Changed += Render;
            _remote.Applied += Render;
            _view.Clicked += OnClicked;
            Render();
        }

        public void Dispose()
        {
            _collection.Candy.Changed -= OnCandyChanged;
            _localization.Changed -= Render;
            _remote.Applied -= Render;
            _view.Clicked -= OnClicked;
        }

        private void OnCandyChanged(int candy)
        {
            Render();
            _view.PunchCandy();
        }

        private void OnClicked()
        {
            _ui.PlayClick();
            _analytics.EventOpened();
            _navigation.Show(MenuScreen.Halloween);
        }

        private void Render()
        {
            _view.SetVisible(_collection.IsRunning);
            if (!_collection.IsRunning)
                return;

            var left = _collection.DaysLeft;
            var days = left > 0 ? _localization.Get(LocalizationTable.Ui, DaysKey, left) : _localization.Get(LocalizationTable.Ui, LastDayKey);
            _view.SetContent(_localization.Get(LocalizationTable.Ui, TitleKey), days, _collection.Candy.Value.ToString());
        }
    }
}
