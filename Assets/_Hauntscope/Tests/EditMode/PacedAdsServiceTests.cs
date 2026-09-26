using System.Threading;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PacedAdsServiceTests
    {
        private FakeAdsService _inner;
        private AdPacing _pacing;
        private FakeClock _clock;
        private PacedAdsService _ads;

        [SetUp]
        public void SetUp()
        {
            _inner = new FakeAdsService();
            _pacing = new AdPacing();
            _clock = new FakeClock();
            _ads = new PacedAdsService(_inner, _pacing, _clock);
        }

        [Test]
        public void ShowRewarded_Played_RecordsTheTime()
        {
            _ads.ShowRewardedAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(_clock.UtcNow, _pacing.LastAdAt);
        }

        [Test]
        public void ShowRewarded_ClosedEarly_StillCountsAsAnAd()
        {
            _inner.RewardEarned = false;

            var rewarded = _ads.ShowRewardedAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(rewarded);
            Assert.IsTrue(_pacing.LastAdAt.HasValue);
        }

        [Test]
        public void ShowRewarded_NotLoaded_RecordsNothing()
        {
            _inner.IsRewardedReady = false;

            _ads.ShowRewardedAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_pacing.LastAdAt.HasValue);
        }

        [Test]
        public void ShowInterstitial_Played_ResetsTheHuntCounter()
        {
            _pacing.RegisterHunt();
            _pacing.RegisterHunt();

            _ads.ShowInterstitialAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, _pacing.HuntsSinceInterstitial);
            Assert.AreEqual(_clock.UtcNow, _pacing.LastAdAt);
        }

        [Test]
        public void IsShowing_Always_ComesFromTheInnerService()
        {
            _inner.IsShowing = true;

            Assert.IsTrue(_ads.IsShowing);
        }
    }
}
