using System.Threading;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PurchaseAnalyticsTests
    {
        private FakeIapStore _store;
        private FakeAnalyticsService _analytics;
        private PurchaseAnalytics _purchases;

        [SetUp]
        public void SetUp()
        {
            _store = new FakeIapStore();
            _analytics = new FakeAnalyticsService();
            _purchases = new PurchaseAnalytics(_store, _analytics);
            _purchases.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _purchases.Dispose();
        }

        [Test]
        public void Purchase_ClosedByThePlayer_LogsOnlyPurchaseCancelled()
        {
            _store.Outcome = IapPurchaseStatus.Cancelled;

            Buy("starter_pack");

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.PurchaseCancelled));
            Assert.AreEqual("starter_pack", _analytics.ParameterOf(AnalyticsNames.PurchaseCancelled, AnalyticsNames.ProductId));
            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.PurchaseFailed));
        }

        [Test]
        public void Purchase_RefusedByTheStore_LogsPurchaseFailedWithASnakeCaseReason()
        {
            _store.Outcome = IapPurchaseStatus.Failed;
            _store.FailureReason = "PaymentDeclined";

            Buy("ecto_jar");

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.PurchaseFailed));
            Assert.AreEqual("ecto_jar", _analytics.ParameterOf(AnalyticsNames.PurchaseFailed, AnalyticsNames.ProductId));
            Assert.AreEqual("payment_declined", _analytics.ParameterOf(AnalyticsNames.PurchaseFailed, AnalyticsNames.Reason));
            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.PurchaseCancelled));
        }

        [Test]
        public void Purchase_Succeeds_LogsNothing()
        {
            _store.Initialize(new IapProduct[0], order => true);

            Buy("full_version");

            Assert.AreEqual(0, _analytics.Events.Count);
        }

        [Test]
        public void Dispose_AfterStart_StopsListening()
        {
            _purchases.Dispose();
            _store.Outcome = IapPurchaseStatus.Failed;

            Buy("field_kit");

            Assert.AreEqual(0, _analytics.Events.Count);
        }

        private void Buy(string productId)
        {
            _store.PurchaseAsync(productId, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
