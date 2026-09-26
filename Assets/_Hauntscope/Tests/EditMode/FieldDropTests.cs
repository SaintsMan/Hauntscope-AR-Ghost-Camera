using System;
using System.Threading;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class FieldDropTests
    {
        private const int Reward = 30;
        private const int PerDay = 3;

        private FakeAdsService _ads;
        private FakeClock _clock;
        private PlayerProgress _progress;
        private PlayerProgressRepository _repository;
        private FieldDrop _drop;

        [SetUp]
        public void SetUp()
        {
            _ads = new FakeAdsService();
            _clock = new FakeClock();
            _progress = new PlayerProgress();
            _repository = new PlayerProgressRepository(new FakeSaveService());
            _drop = new FieldDrop(_ads, _progress, _repository, _clock, new AdsConfig(3, 150f, 3, Reward, PerDay));
        }

        [TearDown]
        public void TearDown()
        {
            _drop.Dispose();
        }

        [Test]
        public void Claim_RewardEarned_PaysAndSaves()
        {
            var claimed = Claim();

            Assert.IsTrue(claimed);
            Assert.AreEqual(Reward, _progress.Ectoplasm.Value);
            Assert.AreEqual(Reward, _repository.Load().Ectoplasm.Value);
            Assert.AreEqual(PerDay - 1, _drop.RemainingToday);
        }

        [Test]
        public void Claim_AdClosedEarly_PaysNothing()
        {
            _ads.RewardEarned = false;

            var claimed = Claim();

            Assert.IsFalse(claimed);
            Assert.AreEqual(0, _progress.Ectoplasm.Value);
            Assert.AreEqual(PerDay, _drop.RemainingToday);
        }

        [Test]
        public void Claim_DailyLimitReached_ShowsNoMoreAds()
        {
            for (var i = 0; i < PerDay; i++)
                Claim();

            var claimed = Claim();

            Assert.IsFalse(claimed);
            Assert.AreEqual(PerDay, _ads.RewardedShown);
            Assert.IsFalse(_drop.CanClaim);
        }

        [Test]
        public void RemainingToday_NextDay_ResetsTheLimit()
        {
            for (var i = 0; i < PerDay; i++)
                Claim();

            _clock.Today = _clock.Today.AddDays(1);

            Assert.AreEqual(PerDay, _drop.RemainingToday);
            Assert.IsTrue(_drop.CanClaim);
        }

        [Test]
        public void CanClaim_NoAdLoaded_IsFalse()
        {
            _ads.IsRewardedReady = false;

            Assert.IsFalse(_drop.CanClaim);
        }

        [Test]
        public void DayKey_Always_IsYearMonthDay()
        {
            Assert.AreEqual(20261031, FieldDrop.DayKey(new DateTime(2026, 10, 31)));
        }

        private bool Claim()
        {
            return _drop.ClaimAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
