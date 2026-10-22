using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Gameplay.Onboarding;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class MenuStoreLinkPresenter : IStartable, IDisposable
    {
        private readonly MenuStoreLinkView _view;
        private readonly MenuNavigation _navigation;
        private readonly StarterOffer _starter;
        private readonly PaidStore _paid;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly FeatureUnlocks _features;

        public MenuStoreLinkPresenter(MenuStoreLinkView view, MenuNavigation navigation, StarterOffer starter, PaidStore paid,
            ILocalizationService localization, UiFeedback ui, FeatureUnlocks features)
        {
            _features = features;
            _view = view;
            _navigation = navigation;
            _starter = starter;
            _paid = paid;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _view.BalanceClicked += OnBalanceClicked;
            _paid.Changed += Render;
            _localization.Changed += Render;
            _navigation.Current.Changed += OnScreenChanged;
            Render();
        }

        public void Dispose()
        {
            _view.BalanceClicked -= OnBalanceClicked;
            _paid.Changed -= Render;
            _localization.Changed -= Render;
            _navigation.Current.Changed -= OnScreenChanged;
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            Render();
        }

        private void Render()
        {
            var product = _starter.Product;
            var badge = _starter.IsActive && !string.IsNullOrEmpty(product.BadgeKey)
                ? _localization.Get(LocalizationTable.Ui, product.BadgeKey)
                : string.Empty;
            _view.SetOffer(badge);
        }

        // The balance is shown before the shop opens; until then it is just a number.
        private void OnBalanceClicked()
        {
            if (!_features.IsUnlocked(Feature.Shop))
            {
                _ui.PlayDenied();
                return;
            }

            _ui.PlayClick();
            _features.NotifyUsed(Feature.Shop);
            _navigation.ShowShop(ShopTab.Supplies);
        }
    }
}
