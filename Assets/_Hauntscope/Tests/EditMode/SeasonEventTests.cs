using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Season;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SeasonEventTests
    {
        private FakeRemoteValues _remote;
        private FakeClock _clock;
        private SeasonEvent _season;
        private DemoSwitches _demo;

        [SetUp]
        public void SetUp()
        {
            _remote = new FakeRemoteValues();
            _clock = new FakeClock { Today = new DateTime(2026, 10, 28) };
            _demo = DemoSwitches.Off;
            _season = Create();
        }

        [Test]
        public void IsHalloween_NoKeyFromTheConsole_IsOff()
        {
            Assert.IsFalse(_season.IsHalloween);
        }

        [Test]
        public void IsHalloween_ConsoleSaysHalloween_IsOn()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = "halloween";

            Assert.IsTrue(_season.IsHalloween);
        }

        [Test]
        public void IsHalloween_ConsoleKeyInOtherCase_IsStillOn()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = " Halloween ";

            Assert.IsTrue(_season.IsHalloween);
        }

        [Test]
        public void IsHalloween_ConsoleSwitchesItOff_StopsTheEvent()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = "halloween";
            _remote.Values[RemoteKeys.SeasonEvent] = string.Empty;

            Assert.IsFalse(_season.IsHalloween);
        }

        [Test]
        public void IsHalloween_AfterTheLastDay_StopsEvenIfTheKeyStays()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = "halloween";
            _clock.Today = new DateTime(2026, 11, 3);

            Assert.IsFalse(_season.IsHalloween);
        }

        [Test]
        public void IsHalloween_OnTheLastDay_StillRuns()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = "halloween";
            _clock.Today = new DateTime(2026, 11, 2);

            Assert.IsTrue(_season.IsHalloween);
        }

        [Test]
        public void HalloweenEnd_ConsoleMovesIt_ExtendsTheEvent()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = "halloween";
            _remote.Values[RemoteKeys.HalloweenEnd] = "2026-11-09";
            _clock.Today = new DateTime(2026, 11, 5);

            Assert.IsTrue(_season.IsHalloween);
            Assert.AreEqual(4, _season.HalloweenDaysLeft);
        }

        [Test]
        public void HalloweenEnd_ConsoleSendsGarbage_KeepsTheConfigsDate()
        {
            _remote.Values[RemoteKeys.HalloweenEnd] = "next week";

            Assert.AreEqual(new DateTime(2026, 11, 2), _season.HalloweenEnd);
        }

        [Test]
        public void HalloweenDaysLeft_FiveDaysBefore_IsFive()
        {
            _clock.Today = new DateTime(2026, 10, 28);

            Assert.AreEqual(5, _season.HalloweenDaysLeft);
        }

        [Test]
        public void Allows_NoSeasonOnTheThing_IsAlwaysTrue()
        {
            Assert.IsTrue(_season.Allows(string.Empty));
        }

        [Test]
        public void Allows_SeasonalThingOutsideTheSeason_IsFalse()
        {
            Assert.IsFalse(_season.Allows("halloween"));
        }

        [Test]
        public void IsHalloween_DemoSwitchOn_RunsWithNoConsoleAndNoCalendar()
        {
            _demo = new DemoSwitchesBuilder().Halloween().Build();
            _clock.Today = new DateTime(2027, 3, 1);

            Assert.IsTrue(Create().IsHalloween);
        }

        [Test]
        public void GhostPercent_OutsideTheEvent_IsZero()
        {
            Assert.AreEqual(0, _season.GhostPercent("halloween"));
        }

        [Test]
        public void GhostPercent_DuringTheEvent_UsesTheConfigsDefault()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = "halloween";

            Assert.AreEqual(35, _season.GhostPercent("halloween"));
        }

        [Test]
        public void GhostPercent_ConsoleSetsIt_UsesTheConsolesValue()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = "halloween";
            _remote.Values[RemoteKeys.HalloweenJackPercent] = "60";

            Assert.AreEqual(60, _season.GhostPercent("halloween"));
        }

        [Test]
        public void GhostPercent_ConsoleSendsOutOfRange_KeepsTheDefault()
        {
            _remote.Values[RemoteKeys.SeasonEvent] = "halloween";
            _remote.Values[RemoteKeys.HalloweenJackPercent] = "250";

            Assert.AreEqual(35, _season.GhostPercent("halloween"));
        }

        [Test]
        public void PickupCount_ConsoleNamesTheNumber_UsesIt()
        {
            _remote.Values[RemoteKeys.HalloweenCandyPerHunt] = "1";

            Assert.AreEqual(1, _season.PickupCount("halloween", 3));
        }

        [Test]
        public void PickupCount_NotASeasonalPickup_IsLeftAlone()
        {
            _remote.Values[RemoteKeys.HalloweenCandyPerHunt] = "1";

            Assert.AreEqual(3, _season.PickupCount(string.Empty, 3));
        }

        [Test]
        public void SkinPrice_ConsoleSetsIt_UsesTheConsolesPrice()
        {
            var skin = new HalloweenSkin("beam", HalloweenSkinSlot.Beam, 15, UnityEngine.Color.white);
            _remote.Values[RemoteKeys.HalloweenPricePrefix + "beam"] = "9";

            Assert.AreEqual(9, _season.SkinPrice(skin));
        }

        [Test]
        public void SkinPrice_NoValueFromTheConsole_IsTheSkinsOwn()
        {
            var skin = new HalloweenSkin("beam", HalloweenSkinSlot.Beam, 15, UnityEngine.Color.white);

            Assert.AreEqual(15, _season.SkinPrice(skin));
        }

        private SeasonEvent Create()
        {
            return new SeasonEvent(new RemoteTuning(_remote), new LiveOpsConfig(), new HalloweenConfig(), _clock, _demo);
        }
    }
}
