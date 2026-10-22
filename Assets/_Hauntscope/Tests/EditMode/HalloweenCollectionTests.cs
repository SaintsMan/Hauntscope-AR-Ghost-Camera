using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Season;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HalloweenCollectionTests
    {
        private HalloweenFixture _event;

        [SetUp]
        public void SetUp()
        {
            _event = new HalloweenFixture();
        }

        [Test]
        public void Start_EventRunning_GivesTheWelcomeCandyOnce()
        {
            _event.Remote.Values[RemoteKeys.HalloweenWelcomeCandy] = "10";
            var progress = new HalloweenProgress();
            var collection = new HalloweenCollection(progress, _event.Repository, _event.Season, _event.Config, _event.Clock, _event.Demo, _event.Remote);

            collection.Start();
            collection.Start();

            Assert.AreEqual(10, collection.Candy.Value);
            Assert.IsTrue(_event.Repository.Load().IsWelcomed);
        }

        [Test]
        public void RemoteApplied_EventSwitchedOnAfterStart_GivesTheWelcomeCandy()
        {
            _event.Remote.Values[RemoteKeys.SeasonEvent] = string.Empty;
            _event.Remote.Values[RemoteKeys.HalloweenWelcomeCandy] = "10";
            var collection = new HalloweenCollection(new HalloweenProgress(), _event.Repository, _event.Season, _event.Config, _event.Clock,
                _event.Demo, _event.Remote);
            collection.Start();

            _event.Remote.Values[RemoteKeys.SeasonEvent] = "halloween";
            _event.Remote.Apply();

            Assert.AreEqual(10, collection.Candy.Value);
        }

        [Test]
        public void Start_EventOff_GivesNoWelcomeCandy()
        {
            _event.Remote.Values[RemoteKeys.SeasonEvent] = string.Empty;
            _event.Remote.Values[RemoteKeys.HalloweenWelcomeCandy] = "10";
            var progress = new HalloweenProgress();
            var collection = new HalloweenCollection(progress, _event.Repository, _event.Season, _event.Config, _event.Clock, _event.Demo, _event.Remote);

            collection.Start();

            Assert.AreEqual(0, collection.Candy.Value);
            Assert.IsFalse(progress.IsWelcomed);
        }

        [Test]
        public void Add_EventRunning_FillsThePurse()
        {
            _event.Collection.Add(3);

            Assert.AreEqual(3, _event.Collection.Candy.Value);
        }

        [Test]
        public void Add_EventRunning_ReportsTheCandy()
        {
            var reported = 0;
            _event.Collection.CandyAdded += count => reported += count;

            _event.Collection.Add(2);

            Assert.AreEqual(2, reported);
        }

        [Test]
        public void Add_ConsoleSwitchedTheEventOff_TakesNothing()
        {
            _event.Remote.Values[RemoteKeys.SeasonEvent] = string.Empty;

            _event.Collection.Add(3);

            Assert.AreEqual(0, _event.Collection.Candy.Value);
        }

        [Test]
        public void Add_Zero_ChangesNothingAndSavesNothing()
        {
            var saved = _event.Save.SaveCount;

            _event.Collection.Add(0);

            Assert.AreEqual(saved, _event.Save.SaveCount);
        }

        [Test]
        public void Buy_EnoughCandy_PaysAndPutsTheSkinOn()
        {
            _event.Collection.Add(HalloweenFixture.BeamPrice + 2);

            var result = _event.Collection.Buy(_event.Beam);

            Assert.AreEqual(SkinPurchase.Bought, result);
            Assert.AreEqual(2, _event.Collection.Candy.Value);
            Assert.IsTrue(_event.Collection.IsWorn(_event.Beam));
        }

        [Test]
        public void Buy_TooLittleCandy_ChangesNothing()
        {
            _event.Collection.Add(HalloweenFixture.BeamPrice - 1);

            var result = _event.Collection.Buy(_event.Beam);

            Assert.AreEqual(SkinPurchase.NotEnoughCandy, result);
            Assert.IsFalse(_event.Collection.Owns(_event.Beam));
            Assert.AreEqual(HalloweenFixture.BeamPrice - 1, _event.Collection.Candy.Value);
        }

        [Test]
        public void Buy_AlreadyOwned_DoesNotChargeTwice()
        {
            _event.Collection.Add(20);
            _event.Collection.Buy(_event.Beam);

            var result = _event.Collection.Buy(_event.Beam);

            Assert.AreEqual(SkinPurchase.AlreadyOwned, result);
            Assert.AreEqual(20 - HalloweenFixture.BeamPrice, _event.Collection.Candy.Value);
        }

        [Test]
        public void Buy_ConsoleSetsAPrice_CostsThat()
        {
            _event.Remote.Values[RemoteKeys.HalloweenPricePrefix + HalloweenFixture.BeamId] = "2";
            _event.Collection.Add(3);

            _event.Collection.Buy(_event.Beam);

            Assert.AreEqual(1, _event.Collection.Candy.Value);
        }

        [Test]
        public void Buy_EventOver_SellsNothing()
        {
            _event.Collection.Add(20);
            _event.Remote.Values[RemoteKeys.SeasonEvent] = string.Empty;

            var result = _event.Collection.Buy(_event.Beam);

            Assert.AreEqual(SkinPurchase.EventOver, result);
        }

        [Test]
        public void Buy_Bought_ReportsTheSkin()
        {
            HalloweenSkin unlocked = null;
            _event.Collection.SkinUnlocked += skin => unlocked = skin;
            _event.Collection.Add(20);

            _event.Collection.Buy(_event.Lens);

            Assert.AreSame(_event.Lens, unlocked);
        }

        [Test]
        public void Toggle_WornSkin_TakesItOff()
        {
            _event.Collection.Add(20);
            _event.Collection.Buy(_event.Beam);

            _event.Collection.Toggle(_event.Beam);

            Assert.IsNull(_event.Collection.WornIn(HalloweenSkinSlot.Beam));
            Assert.IsTrue(_event.Collection.Owns(_event.Beam));
        }

        [Test]
        public void Toggle_NotOwned_DoesNothing()
        {
            _event.Collection.Toggle(_event.Beam);

            Assert.IsNull(_event.Collection.WornIn(HalloweenSkinSlot.Beam));
        }

        [Test]
        public void Buy_TwoSkinsOfOneSlot_WearsTheNewest()
        {
            var second = new HalloweenSkin("beam_green", HalloweenSkinSlot.Beam, 1, UnityEngine.Color.green);
            var config = new HalloweenConfig(35, 1, new[] { _event.Beam, second });
            var collection = new HalloweenCollection(_event.Progress, _event.Repository, _event.Season, config, _event.Clock, _event.Demo, _event.Remote);
            collection.Start();
            collection.Add(20);
            collection.Buy(_event.Beam);

            collection.Buy(second);

            Assert.AreSame(second, collection.WornIn(HalloweenSkinSlot.Beam));
            Assert.IsTrue(collection.Owns(_event.Beam));
        }

        [Test]
        public void Start_AfterTheSave_RemembersCandySkinsAndWhatIsWorn()
        {
            _event.Collection.Add(20);
            _event.Collection.Buy(_event.Radar);

            var again = _event.Reload();

            Assert.AreEqual(20 - HalloweenFixture.RadarPrice, again.Candy.Value);
            Assert.IsTrue(again.Owns(_event.Radar));
            Assert.IsTrue(again.IsWorn(_event.Radar));
        }

        [Test]
        public void Start_DayAfterTheEnd_BurnsTheCandyButKeepsTheSkins()
        {
            _event.Collection.Add(20);
            _event.Collection.Buy(_event.Beam);
            _event.Clock.Today = new DateTime(2026, 11, 3);

            var again = _event.Reload();

            Assert.AreEqual(0, again.Candy.Value);
            Assert.IsTrue(again.Owns(_event.Beam));
            Assert.IsTrue(again.IsWorn(_event.Beam));
        }

        [Test]
        public void Start_OnTheLastDay_KeepsTheCandy()
        {
            _event.Collection.Add(7);
            _event.Clock.Today = new DateTime(2026, 11, 2);

            var again = _event.Reload();

            Assert.AreEqual(7, again.Candy.Value);
        }

        [Test]
        public void Start_BurnedCandyStaysBurnedWhenTheDateIsMovedBack()
        {
            _event.Collection.Add(7);
            _event.Clock.Today = new DateTime(2026, 11, 3);
            _event.Reload();
            _event.Clock.Today = new DateTime(2026, 10, 30);

            var again = _event.Reload();

            Assert.AreEqual(0, again.Candy.Value);
        }

        [Test]
        public void Start_DemoCandy_FillsThePurse()
        {
            var demo = new DemoSwitchesBuilder().Halloween().Candy(40).Build();
            var fixture = new HalloweenFixture(demo: demo);

            Assert.AreEqual(40, fixture.Collection.Candy.Value);
            Assert.IsTrue(fixture.Collection.IsRunning);
        }

        [Test]
        public void Start_DemoCandyBelowWhatIsInThePurse_DoesNotTakeAnything()
        {
            var demo = new DemoSwitchesBuilder().Halloween().Candy(10).Build();
            var fixture = new HalloweenFixture(demo: demo);
            fixture.Collection.Add(30);

            var again = fixture.Reload();

            Assert.AreEqual(40, again.Candy.Value);
        }

        [Test]
        public void Load_UnknownVersion_StartsFresh()
        {
            _event.Save.SetRaw("halloween", "{\"_version\":99,\"_candy\":50}");

            var progress = _event.Repository.Load();

            Assert.AreEqual(0, progress.Candy);
        }
    }
}
