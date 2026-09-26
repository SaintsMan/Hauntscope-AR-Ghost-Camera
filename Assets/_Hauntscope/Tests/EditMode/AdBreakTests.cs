using System.Collections.Generic;
using System.Threading;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class AdBreakTests
    {
        private FakeAdsService _inner;
        private AdPacing _pacing;
        private AdBreak _adBreak;

        [SetUp]
        public void SetUp()
        {
            _inner = new FakeAdsService();
            _pacing = new AdPacing();
            var clock = new FakeClock();
            var ads = new PacedAdsService(_inner, _pacing, clock);
            var progress = new PlayerProgress(0, new Dictionary<string, int>(), 10, false);
            var policy = new InterstitialPolicy(_pacing, progress, clock, new AdsConfig(3, 150f, 3, 30, 3));
            _adBreak = new AdBreak(ads, _pacing, policy);
        }

        [Test]
        public void TryShow_FirstTwoHunts_ShowsNothing()
        {
            Leave(2);

            Assert.AreEqual(0, _inner.InterstitialsShown);
        }

        [Test]
        public void TryShow_ThirdHunt_ShowsOneInterstitial()
        {
            Leave(3);

            Assert.AreEqual(1, _inner.InterstitialsShown);
        }

        [Test]
        public void TryShow_RightAfterAnInterstitial_WaitsForTheNextCycle()
        {
            Leave(4);

            Assert.AreEqual(1, _inner.InterstitialsShown);
            Assert.AreEqual(1, _pacing.HuntsSinceInterstitial);
        }

        [Test]
        public void TryShow_NoAdLoaded_ShowsNothingAndKeepsCounting()
        {
            _inner.IsInterstitialReady = false;

            Leave(3);

            Assert.AreEqual(0, _inner.InterstitialsShown);
            Assert.AreEqual(3, _pacing.HuntsSinceInterstitial);
        }

        private void Leave(int hunts)
        {
            for (var i = 0; i < hunts; i++)
                _adBreak.TryShowAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
