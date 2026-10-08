using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Iap;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class SuppliesPresenter : IStartable, IDisposable
    {
        private const string PerksKey = "iap.full_version.perks";
        private const string OwnedKey = "iap.full_version.owned";
        private const string AmountKey = "iap.amount";
        private const string BonusKey = "iap.bonus";
        private const string OwnedProductKey = "iap.owned";
        private static readonly TimeSpan ClockTick = TimeSpan.FromSeconds(1);

        private readonly SuppliesView _view;
        private readonly MenuNavigation _navigation;
        private readonly PaidStore _paid;
        private readonly StarterOffer _starter;
        private readonly IapConfig _config;
        private readonly IapCheckout _checkout;
        private readonly ILocalizationService _localization;
        private readonly List<IapProductView> _cards = new List<IapProductView>();
        private readonly List<IapProductData> _products = new List<IapProductData>();
        private readonly List<Action> _handlers = new List<Action>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public SuppliesPresenter(SuppliesView view, MenuNavigation navigation, PaidStore paid, StarterOffer starter, IapConfig config,
            IapCheckout checkout, ILocalizationService localization)
        {
            _view = view;
            _navigation = navigation;
            _paid = paid;
            _starter = starter;
            _config = config;
            _checkout = checkout;
            _localization = localization;
        }

        public void Start()
        {
            foreach (var pack in _config.EctoplasmPacks)
                AddCard(_view.AddPack(), pack);
            foreach (var product in _config.Supplies)
                AddCard(_view.AddKit(), product);

            _view.Premium.BuyClicked += OnPremiumClicked;
            _view.Starter.BuyClicked += OnStarterClicked;
            _paid.Changed += Render;
            _localization.Changed += Render;
            _starter.Refresh();
            Render();
            TickClockAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            for (var i = 0; i < _cards.Count; i++)
                _cards[i].BuyClicked -= _handlers[i];

            _view.Premium.BuyClicked -= OnPremiumClicked;
            _view.Starter.BuyClicked -= OnStarterClicked;
            _paid.Changed -= Render;
            _localization.Changed -= Render;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void AddCard(IapProductView card, IapProductData product)
        {
            Action handler = () => BuyAsync(product, card, _lifetime.Token).Forget();
            card.BuyClicked += handler;
            _cards.Add(card);
            _products.Add(product);
            _handlers.Add(handler);
        }

        private void Render()
        {
            RenderPremium();
            RenderStarter();
            for (var i = 0; i < _cards.Count; i++)
                RenderCard(_products[i], _cards[i]);
        }

        private void RenderPremium()
        {
            var product = _config.FullVersion;
            var premium = _view.Premium;
            premium.SetContent(Store(product.NameKey), Ui(PerksKey, product.Ectoplasm));
            premium.SetPrice(_checkout.PriceOf(product), _checkout.IsAvailable(product));
            premium.SetOwned(_paid.IsOwned(product), Ui(OwnedKey));
        }

        private void RenderStarter()
        {
            var product = _config.StarterPack;
            var card = _view.Starter;
            card.SetVisible(_starter.IsActive);
            if (!_starter.IsActive)
                return;

            card.SetContent(Store(product.NameKey), Store(product.DescriptionKey), Ui(product.BadgeKey));
            card.SetPrice(_checkout.PriceOf(product), _checkout.IsAvailable(product));
            card.SetTimer(_checkout.TimeLeft(_starter.Remaining));
        }

        private void RenderCard(IapProductData product, IapProductView card)
        {
            var detail = product is EctoplasmPackData pack ? Ui(AmountKey, pack.Ectoplasm) : Store(product.DescriptionKey);
            card.SetContent(product.Icon, product.Accent, Store(product.NameKey), detail);
            card.SetRibbon(Ribbon(product), product.Accent);
            if (_paid.IsOwned(product))
                card.SetPrice(Ui(OwnedProductKey), false);
            else
                card.SetPrice(_checkout.PriceOf(product), _checkout.IsAvailable(product));
        }

        // A named deal ("BEST VALUE") wins over the bare bonus.
        private string Ribbon(IapProductData product)
        {
            if (!string.IsNullOrEmpty(product.BadgeKey))
                return Ui(product.BadgeKey);
            return product is EctoplasmPackData pack && pack.BonusPercent > 0 ? Ui(BonusKey, pack.BonusPercent) : string.Empty;
        }

        private async UniTaskVoid TickClockAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_navigation.Current.Value == MenuScreen.Shop && _view.Starter.isActiveAndEnabled)
                {
                    if (_starter.IsActive)
                        _view.Starter.SetTimer(_checkout.TimeLeft(_starter.Remaining));
                    else
                        RenderStarter();
                }

                await UniTask.Delay(ClockTick, DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);
            }
        }

        private void OnPremiumClicked()
        {
            BuyPremiumAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid BuyPremiumAsync(CancellationToken cancellationToken)
        {
            var status = await _checkout.BuyAsync(_config.FullVersion, cancellationToken);
            if (status == IapPurchaseStatus.Purchased)
                _view.Premium.PlayPurchased();
            else if (status == IapPurchaseStatus.Failed || status == IapPurchaseStatus.Unavailable)
                _view.Premium.PlayDenied();
        }

        private void OnStarterClicked()
        {
            BuyStarterAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid BuyStarterAsync(CancellationToken cancellationToken)
        {
            var status = await _checkout.BuyAsync(_config.StarterPack, cancellationToken);
            if (status == IapPurchaseStatus.Failed || status == IapPurchaseStatus.Unavailable)
                _view.Starter.PlayDenied();
        }

        private async UniTaskVoid BuyAsync(IapProductData product, IapProductView card, CancellationToken cancellationToken)
        {
            var status = await _checkout.BuyAsync(product, cancellationToken);
            if (status == IapPurchaseStatus.Purchased)
                card.PlayPurchased();
            else if (status == IapPurchaseStatus.Failed || status == IapPurchaseStatus.Unavailable)
                card.PlayDenied();
        }

        private string Store(string key)
        {
            return _localization.Get(LocalizationTable.Store, key);
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }
    }
}
