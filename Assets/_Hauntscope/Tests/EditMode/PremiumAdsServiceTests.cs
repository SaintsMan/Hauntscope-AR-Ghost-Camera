using System.Threading;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PremiumAdsServiceTests
    {
        private FakeAdsService _inner;
        private PurchaseHistory _history;
        private PremiumAdsService _ads;

        [SetUp]
        public void SetUp()
        {
            _inner = new FakeAdsService();
            _history = new PurchaseHistory();
            _ads = new PremiumAdsService(_inner, _history);
        }

        [TearDown]
        public void TearDown()
        {
            _ads.Dispose();
        }

        [Test]
        public void ShowRewardedAsync_FreePlayer_PlaysTheAd()
        {
            var rewarded = _ads.ShowRewardedAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(rewarded);
            Assert.AreEqual(1, _inner.RewardedShown);
        }

        [Test]
        public void ShowRewardedAsync_FullVersion_RewardsWithoutAnAd()
        {
            _history.SetPremium();
            _inner.IsRewardedReady = false;

            var rewarded = _ads.ShowRewardedAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(rewarded);
            Assert.IsTrue(_ads.IsRewardedReady);
            Assert.AreEqual(0, _inner.RewardedShown);
        }

        [Test]
        public void ShowInterstitialAsync_FullVersion_ShowsNothing()
        {
            _history.SetPremium();

            var shown = _ads.ShowInterstitialAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(shown);
            Assert.IsFalse(_ads.IsInterstitialReady);
            Assert.AreEqual(0, _inner.InterstitialsShown);
        }

        [Test]
        public void AvailabilityChanged_OnBuyingFullVersion_IsRaised()
        {
            var raised = 0;
            _ads.AvailabilityChanged += () => raised++;

            _history.SetPremium();

            Assert.AreEqual(1, raised);
        }
    }
}
