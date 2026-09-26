using System.Threading;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class LoginCalendarTests
    {
        private StoreFixture _store;
        private FakeClock _clock;
        private FakeAdsService _ads;
        private EngagementProgress _engagement;
        private EngagementRepository _repository;
        private LoginCalendar _calendar;

        [SetUp]
        public void SetUp()
        {
            _store = new StoreFixture();
            _clock = new FakeClock();
            _ads = new FakeAdsService();
            var config = new LoginConfig(new[]
            {
                new LoginReward(15),
                new LoginReward(0, new GearReward(_store.Battery, 1)),
                new LoginReward(25),
                new LoginReward(0, new GearReward(_store.Amp, 1), new GearReward(_store.Salt, 1)),
                new LoginReward(40),
                new LoginReward(0, new GearReward(_store.Salt, 2)),
                new LoginReward(80, new GearReward(_store.Battery, 2))
            }, 2);
            _engagement = new EngagementProgress();
            _repository = new EngagementRepository(_store.Save, new ContractConfig(), _store.Config);
            var granter = new RewardGranter(_store.Progress, _store.ProgressRepository, _store.Inventory, _store.InventoryRepository, _store.Config);
            _calendar = new LoginCalendar(_engagement, _repository, config, granter, _clock, _ads);
        }

        [TearDown]
        public void TearDown()
        {
            _calendar.Dispose();
        }

        [Test]
        public void Claim_FirstDay_PaysTheFirstFrame()
        {
            var bundle = _calendar.Claim();

            Assert.AreEqual(15, bundle.Ectoplasm);
            Assert.AreEqual(15, _store.Progress.Ectoplasm.Value);
            Assert.IsFalse(_calendar.CanClaim);
        }

        [Test]
        public void Claim_SameDayAgain_PaysNothing()
        {
            _calendar.Claim();

            Assert.IsNull(_calendar.Claim());
            Assert.AreEqual(15, _store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void Claim_NextDay_PaysTheNextFrame()
        {
            _calendar.Claim();
            NextDay();

            _calendar.Claim();

            Assert.AreEqual(1, _store.Inventory.GetCount(StoreFixture.BatteryId));
        }

        [Test]
        public void Claim_AfterMissedDays_ContinuesWhereItStopped()
        {
            _calendar.Claim();
            _clock.Today = _clock.Today.AddDays(5);

            Assert.AreEqual(1, _calendar.TodayIndex);
        }

        [Test]
        public void Claim_ClockSetBack_PaysNothing()
        {
            _calendar.Claim();
            _clock.Today = _clock.Today.AddDays(-3);

            Assert.IsFalse(_calendar.CanClaim);
            Assert.IsNull(_calendar.Claim());
        }

        [Test]
        public void Claim_TwoGearFrame_PaysBoth()
        {
            ClaimDays(3);
            NextDay();

            _calendar.Claim();

            Assert.AreEqual(1, _store.Inventory.GetCount(StoreFixture.AmpId));
            Assert.AreEqual(1, _store.Inventory.GetCount(StoreFixture.SaltId));
        }

        [Test]
        public void Claim_AfterTheLastFrame_StartsTheCassetteAgain()
        {
            ClaimDays(7);
            NextDay();

            Assert.AreEqual(0, _calendar.TodayIndex);
            Assert.AreEqual(15, _calendar.TodayReward.Ectoplasm);
        }

        [Test]
        public void TodayIndex_ClaimedToday_StaysOnTodaysFrame()
        {
            ClaimDays(3);

            Assert.AreEqual(2, _calendar.TodayIndex);
            Assert.IsTrue(_calendar.IsClaimedToday);
        }

        [Test]
        public void ClaimDoubled_AdWatched_DoublesTheEctoplasm()
        {
            var bundle = _calendar.ClaimDoubledAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(30, bundle.Ectoplasm);
            Assert.AreEqual(30, _store.Progress.Ectoplasm.Value);
            Assert.AreEqual(1, _ads.RewardedShown);
        }

        [Test]
        public void ClaimDoubled_AdSkipped_KeepsTheRationForLater()
        {
            _ads.RewardEarned = false;

            var bundle = _calendar.ClaimDoubledAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsNull(bundle);
            Assert.IsTrue(_calendar.CanClaim);
        }

        [Test]
        public void IsDoubleOffered_GearOnlyFrame_IsFalse()
        {
            _calendar.Claim();
            NextDay();

            Assert.IsFalse(_calendar.IsDoubleOffered);
        }

        [Test]
        public void ShouldOpenByItself_AlreadyShownToday_IsFalse()
        {
            var first = _calendar.ShouldOpenByItself;
            _calendar.MarkShown();

            Assert.IsTrue(first);
            Assert.IsFalse(_calendar.ShouldOpenByItself);
            Assert.IsTrue(_calendar.CanClaim);
        }

        [Test]
        public void ShouldOpenByItself_NextDay_OpensAgain()
        {
            _calendar.MarkShown();
            NextDay();

            Assert.IsTrue(_calendar.ShouldOpenByItself);
        }

        [Test]
        public void Claim_Always_Saves()
        {
            _calendar.Claim();

            var loaded = _repository.Load();
            Assert.AreEqual(1, loaded.LoginClaims);
            Assert.AreEqual(20261031, loaded.LastLoginDay);
        }

        private void ClaimDays(int days)
        {
            for (var i = 0; i < days; i++)
            {
                if (i > 0)
                    NextDay();
                _calendar.Claim();
            }
        }

        private void NextDay()
        {
            _clock.Today = _clock.Today.AddDays(1);
        }
    }
}
