using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Iap;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // The paid offer OfferFunnel picks on the way back from a hunt, in the popup queue after the free things (briefing,
    // ration). Never on a cold start, at most one per visit, and "not now" is always one tap away.
    public sealed class OfferPopupPresenter : IStartable, IDisposable, IMenuPopup
    {
        private static readonly TimeSpan ClockTick = TimeSpan.FromSeconds(1);

        private readonly OfferPopupView _view;
        private readonly MenuPopupQueue _popups;
        private readonly StarterOffer _starter;
        private readonly OfferFunnel _funnel;
        private readonly PaidStore _paid;
        private readonly IapCheckout _checkout;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private IapProductData _offer;

        public OfferPopupPresenter(OfferPopupView view, MenuPopupQueue popups, StarterOffer starter, OfferFunnel funnel, PaidStore paid,
            IapCheckout checkout, ILocalizationService localization, UiFeedback ui)
        {
            _view = view;
            _popups = popups;
            _starter = starter;
            _funnel = funnel;
            _paid = paid;
            _checkout = checkout;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _view.SetVisible(false);
            _view.BuyClicked += OnBuyClicked;
            _view.LaterClicked += OnLaterClicked;
            _paid.Changed += Render;
            _localization.Changed += Render;

            _offer = _funnel.Pick();
            if (_offer != null)
                _popups.Enqueue(this);
        }

        public void Dispose()
        {
            _view.BuyClicked -= OnBuyClicked;
            _view.LaterClicked -= OnLaterClicked;
            _paid.Changed -= Render;
            _localization.Changed -= Render;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public void Open()
        {
            Render();
            _view.SetVisible(true);
            TickClockAsync(_lifetime.Token).Forget();
        }

        public void Close()
        {
            _view.SetVisible(false);
            _popups.NotifyClosed(this);
        }

        private bool IsStarter => _offer == _starter.Product;

        private void Render()
        {
            if (_offer == null)
                return;

            var description = _localization.Get(LocalizationTable.Store, _offer.DescriptionKey);
            var body = string.IsNullOrEmpty(_offer.PitchKey)
                ? description
                : _localization.Get(LocalizationTable.Ui, _offer.PitchKey, description);
            var ribbon = string.IsNullOrEmpty(_offer.BadgeKey) ? string.Empty : _localization.Get(LocalizationTable.Ui, _offer.BadgeKey);
            _view.SetContent(_offer.Icon, _offer.Accent, _localization.Get(LocalizationTable.Store, _offer.NameKey), body, ribbon);
            _view.SetTimer(IsStarter ? _checkout.TimeLeft(_starter.Remaining) : string.Empty);
            _view.SetPrice(_checkout.PriceOf(_offer), _checkout.IsAvailable(_offer));
        }

        private async UniTaskVoid TickClockAsync(CancellationToken cancellationToken)
        {
            while (IsStarter && _popups.IsOpen(this) && !cancellationToken.IsCancellationRequested)
            {
                _view.SetTimer(_checkout.TimeLeft(_starter.Remaining));
                await UniTask.Delay(ClockTick, DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);
            }
        }

        private void OnBuyClicked()
        {
            BuyAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid BuyAsync(CancellationToken cancellationToken)
        {
            var status = await _checkout.BuyAsync(_offer, cancellationToken);
            if (status == IapPurchaseStatus.Purchased)
                Close();
            else if (status == IapPurchaseStatus.Failed || status == IapPurchaseStatus.Unavailable)
                _view.PlayDenied();
        }

        private void OnLaterClicked()
        {
            _ui.PlayBack();
            Close();
        }
    }
}
