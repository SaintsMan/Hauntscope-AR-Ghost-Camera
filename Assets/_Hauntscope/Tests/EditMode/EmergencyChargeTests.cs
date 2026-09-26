using System.Threading;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class EmergencyChargeTests
    {
        private const float Countdown = 8f;

        private StoreFixture _store;
        private Battery _battery;
        private HuntPause _pause;
        private EmergencyCharge _emergency;
        private FakeAdsService _ads;

        [SetUp]
        public void SetUp()
        {
            _store = new StoreFixture();
            _battery = new Battery(TestConfigs.Tools(batteryMax: 100f));
            _ads = new FakeAdsService { IsRewardedReady = false };
            _pause = new HuntPause(new FakeTrackingStatus(), new FakeApplicationLifecycle(), new TrackingConfig(0.5f), _ads);
            var spares = new SpareBatteries(_store.Inventory, _store.InventoryRepository, _store.Config, _battery);
            _emergency = new EmergencyCharge(_pause, spares, new EmergencyConfig(Countdown, 0.5f), _ads, _battery);
            _battery.Drain(100f);
        }

        [Test]
        public void TryOffer_NoSpare_LetsTheGhostGo()
        {
            var offered = _emergency.TryOffer();

            Assert.IsFalse(offered);
            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void TryOffer_HasSpare_PausesOnTheCard()
        {
            _store.Inventory.AddGear(StoreFixture.BatteryId, 1);

            var offered = _emergency.TryOffer();

            Assert.IsTrue(offered);
            Assert.AreEqual(EmergencyState.Offered, _emergency.State.Value);
            Assert.IsTrue(_pause.Has(PauseReason.Emergency));
            Assert.AreEqual(Countdown, _emergency.TimeLeft.Value);
        }

        [Test]
        public void UseSpare_Offered_RechargesAndResumes()
        {
            _store.Inventory.AddGear(StoreFixture.BatteryId, 1);
            _emergency.TryOffer();

            _emergency.UseSpare();

            Assert.AreEqual(StoreFixture.BatteryCharge * 100f, _battery.Charge.Value, 1e-3f);
            Assert.AreEqual(EmergencyState.Idle, _emergency.State.Value);
            Assert.IsFalse(_pause.IsPaused);
            Assert.IsFalse(_emergency.IsDeclined);
        }

        [Test]
        public void Tick_CountdownRunsOut_GivesUp()
        {
            _store.Inventory.AddGear(StoreFixture.BatteryId, 1);
            _emergency.TryOffer();

            _emergency.Tick(Countdown + 0.1f);

            Assert.AreEqual(EmergencyState.Idle, _emergency.State.Value);
            Assert.IsTrue(_emergency.IsDeclined);
            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void TryOffer_AfterGivingUp_IsNotOfferedAgain()
        {
            _store.Inventory.AddGear(StoreFixture.BatteryId, 1);
            _emergency.TryOffer();
            _emergency.GiveUp();

            var offered = _emergency.TryOffer();

            Assert.IsFalse(offered);
        }

        [Test]
        public void ResetForHunt_AfterGivingUp_OffersAgain()
        {
            _store.Inventory.AddGear(StoreFixture.BatteryId, 1);
            _emergency.TryOffer();
            _emergency.GiveUp();

            _emergency.ResetForHunt();

            Assert.IsTrue(_emergency.TryOffer());
        }

        [Test]
        public void Tick_NotOffered_DoesNothing()
        {
            _emergency.Tick(Countdown * 2f);

            Assert.AreEqual(EmergencyState.Idle, _emergency.State.Value);
            Assert.IsFalse(_emergency.IsDeclined);
        }

        [Test]
        public void TryOffer_NoSpareButAnAdReady_StillOffers()
        {
            _ads.IsRewardedReady = true;

            Assert.IsTrue(_emergency.TryOffer());
            Assert.IsTrue(_emergency.CanWatchAd);
        }

        [Test]
        public void WatchAd_Rewarded_RechargesAndResumes()
        {
            _ads.IsRewardedReady = true;
            _emergency.TryOffer();

            var recharged = _emergency.WatchAdAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(recharged);
            Assert.AreEqual(50f, _battery.Charge.Value, 1e-3f);
            Assert.AreEqual(EmergencyState.Idle, _emergency.State.Value);
            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void WatchAd_ClosedEarly_ReturnsToTheCard()
        {
            _ads.IsRewardedReady = true;
            _ads.RewardEarned = false;
            _emergency.TryOffer();

            var recharged = _emergency.WatchAdAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(recharged);
            Assert.AreEqual(EmergencyState.Offered, _emergency.State.Value);
            Assert.AreEqual(0f, _battery.Charge.Value);
        }

        [Test]
        public void TryOffer_AdAlreadyUsedThisHunt_OffersOnlyTheSpare()
        {
            _ads.IsRewardedReady = true;
            _emergency.TryOffer();
            _emergency.WatchAdAsync(CancellationToken.None).GetAwaiter().GetResult();
            _battery.Drain(100f);

            var offered = _emergency.TryOffer();

            Assert.IsFalse(offered);
            Assert.IsFalse(_emergency.IsAdOffered);
        }

        [Test]
        public void ResetForHunt_AfterAnAd_OffersTheAdAgain()
        {
            _ads.IsRewardedReady = true;
            _emergency.TryOffer();
            _emergency.WatchAdAsync(CancellationToken.None).GetAwaiter().GetResult();

            _emergency.ResetForHunt();

            Assert.IsTrue(_emergency.CanWatchAd);
        }
    }
}
