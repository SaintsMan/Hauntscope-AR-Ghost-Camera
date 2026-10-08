using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Iap;

namespace Hauntscope.UI.Menu
{
    // What every "buy for money" button in the menu shares: the price text, one purchase at a time and the sounds.
    public sealed class IapCheckout
    {
        private const string PriceUnknownKey = "iap.price_unknown";
        private const string TimerKey = "iap.offer.ends";

        private readonly PaidStore _paid;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private bool _busy;

        public IapCheckout(PaidStore paid, ILocalizationService localization, UiFeedback ui)
        {
            _paid = paid;
            _localization = localization;
            _ui = ui;
        }

        public bool IsAvailable(IapProductData product)
        {
            return _paid.PriceOf(product).Length > 0;
        }

        // Before the store answers there is no honest price to show, only a dash.
        public string PriceOf(IapProductData product)
        {
            var price = _paid.PriceOf(product);
            return price.Length > 0 ? price : _localization.Get(LocalizationTable.Ui, PriceUnknownKey);
        }

        public string TimeLeft(TimeSpan left)
        {
            var clock = $"{(int)left.TotalHours:00}:{left.Minutes:00}:{left.Seconds:00}";
            return _localization.Get(LocalizationTable.Ui, TimerKey, clock);
        }

        // Null while another purchase is still with the store; a cancelled or pending one is neither a win nor an error.
        public async UniTask<IapPurchaseStatus?> BuyAsync(IapProductData product, CancellationToken cancellationToken)
        {
            if (_busy)
                return null;

            _busy = true;
            try
            {
                _ui.PlayClick();
                var status = await _paid.BuyAsync(product, cancellationToken);
                if (status == IapPurchaseStatus.Purchased)
                    _ui.PlayPurchase();
                else if (status == IapPurchaseStatus.Failed || status == IapPurchaseStatus.Unavailable)
                    _ui.PlayDenied();
                return status;
            }
            finally
            {
                _busy = false;
            }
        }
    }
}
