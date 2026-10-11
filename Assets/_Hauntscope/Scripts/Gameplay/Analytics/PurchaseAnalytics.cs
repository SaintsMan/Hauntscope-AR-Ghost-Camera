using System;
using Hauntscope.Core.Services;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // Purchases that did not go through, by the names every Pavko game shares: purchase_cancelled when the player closed
    // the payment sheet, purchase_failed only when the store refused, so a payment problem never hides among changed
    // minds. Successful purchases are reported by Google Play itself (in_app_purchase).
    public sealed class PurchaseAnalytics : IStartable, IDisposable
    {
        private readonly IIapStore _store;
        private readonly IAnalyticsService _analytics;

        public PurchaseAnalytics(IIapStore store, IAnalyticsService analytics)
        {
            _store = store;
            _analytics = analytics;
        }

        public void Start()
        {
            _store.PurchaseFailed += OnPurchaseFailed;
        }

        public void Dispose()
        {
            _store.PurchaseFailed -= OnPurchaseFailed;
        }

        private void OnPurchaseFailed(IapFailure failure)
        {
            if (failure.CancelledByPlayer)
            {
                _analytics.Log(AnalyticsNames.PurchaseCancelled, AnalyticsParameter.Of(AnalyticsNames.ProductId, failure.ProductId));
                return;
            }

            _analytics.Log(AnalyticsNames.PurchaseFailed, AnalyticsParameter.Of(AnalyticsNames.ProductId, failure.ProductId),
                AnalyticsParameter.Of(AnalyticsNames.Reason, AnalyticsNames.SnakeCase(failure.Reason)));
        }
    }
}
