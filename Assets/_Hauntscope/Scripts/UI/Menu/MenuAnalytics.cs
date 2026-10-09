using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // Which menu screens players actually open: one screen_view per change of screen, the shop by its tab.
    public sealed class MenuAnalytics : IStartable, IDisposable
    {
        private readonly MenuNavigation _navigation;
        private readonly IAnalyticsService _analytics;

        public MenuAnalytics(MenuNavigation navigation, IAnalyticsService analytics)
        {
            _navigation = navigation;
            _analytics = analytics;
        }

        public void Start()
        {
            _navigation.Current.Changed += OnScreenChanged;
            _navigation.CurrentShopTab.Changed += OnShopTabChanged;
            OnScreenChanged(_navigation.Current.Value);
        }

        public void Dispose()
        {
            _navigation.Current.Changed -= OnScreenChanged;
            _navigation.CurrentShopTab.Changed -= OnShopTabChanged;
        }

        // ShowShop sets the tab before the screen, so a tab change counts only while the shop is already open.
        private void OnShopTabChanged(ShopTab tab)
        {
            if (_navigation.Current.Value == MenuScreen.Shop)
                OnScreenChanged(MenuScreen.Shop);
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            var name = screen == MenuScreen.Shop
                ? AnalyticsNames.ShopPrefix + AnalyticsNames.Of(_navigation.CurrentShopTab.Value)
                : AnalyticsNames.Of(screen);
            _analytics.Log(AnalyticsNames.ScreenView, AnalyticsParameter.Of(AnalyticsNames.ScreenName, name));
        }
    }
}
