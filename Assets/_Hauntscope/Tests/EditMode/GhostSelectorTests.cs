using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Season;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostSelectorTests
    {
        private GhostData _wisp;
        private GhostData _poltergeist;
        private GhostData _shade;
        private FakeRandom _random;
        private FakeClock _clock;
        private WitchingHour _witchingHour;
        private GhostSelector _selector;

        [SetUp]
        public void SetUp()
        {
            _wisp = CreateGhost("wisp", GhostRarity.Common);
            _poltergeist = CreateGhost("poltergeist", GhostRarity.Common);
            _shade = CreateGhost("shade", GhostRarity.Rare);
            _random = new FakeRandom();
            _clock = new FakeClock { LocalNow = new DateTime(2026, 10, 31, 20, 0, 0) };
            _witchingHour = new WitchingHour(_clock, new NightConfig(23, 4, 2f, 1.25f), new RemoteTuning(new FakeRemoteValues()));
            var config = new GhostConfig(new[] { _poltergeist, _wisp, _shade }, "wisp", 75f, 25f, 0f);
            _selector = new GhostSelector(config, _random, _witchingHour);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_wisp);
            Object.DestroyImmediate(_poltergeist);
            Object.DestroyImmediate(_shade);
        }

        [Test]
        public void Select_FirstSession_ReturnsFirstGhostRegardlessOfRandom()
        {
            _random.DefaultValue = 0.99f;

            var ghost = _selector.Select(huntsPlayed: 0);

            Assert.AreSame(_wisp, ghost);
        }

        [Test]
        public void Select_SecondHunt_ReturnsSecondGhostRegardlessOfRandom()
        {
            _random.DefaultValue = 0.99f;
            var config = new GhostConfig(new[] { _poltergeist, _wisp, _shade }, "wisp", 75f, 25f, 0f, "poltergeist");

            var ghost = new GhostSelector(config, _random, _witchingHour).Select(huntsPlayed: 1);

            Assert.AreSame(_poltergeist, ghost);
        }

        [Test]
        public void Select_RollInFirstCommonShare_ReturnsFirstCommon()
        {
            _random.Enqueue(0.1f);

            Assert.AreSame(_poltergeist, _selector.Select(huntsPlayed: 5));
        }

        [Test]
        public void Select_RollInSecondCommonShare_ReturnsSecondCommon()
        {
            _random.Enqueue(0.5f);

            Assert.AreSame(_wisp, _selector.Select(huntsPlayed: 5));
        }

        [Test]
        public void Select_RollInRareShare_ReturnsRare()
        {
            _random.Enqueue(0.8f);

            Assert.AreSame(_shade, _selector.Select(huntsPlayed: 5));
        }

        [Test]
        public void Select_ManyRolls_MatchesRarityWeights()
        {
            var rare = CountPicks(_selector, _shade);

            Assert.AreEqual(25, rare);
        }

        [Test]
        public void Select_FirstGhostMissing_FallsBackToWeightedRandom()
        {
            var config = new GhostConfig(new[] { _shade }, "wisp", 75f, 25f, 0f);
            var selector = new GhostSelector(config, _random, _witchingHour);

            Assert.AreSame(_shade, selector.Select(huntsPlayed: 0));
        }

        [Test]
        public void Select_RoundWeights_ReplaceTheConfiguredSplit()
        {
            _random.Enqueue(0.1f);

            var ghost = _selector.Select(huntsPlayed: 5, new RarityWeights(0f, 60f, 40f));

            Assert.AreSame(_shade, ghost);
        }

        [Test]
        public void Select_NightOnlyGhostByDay_TakesNoneOfTheRareShare()
        {
            var lurker = CreateGhost("lurker", GhostRarity.Rare, nightOnly: true);
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, _shade, lurker }, "wisp", 75f, 25f, 0f), _random,
                _witchingHour);
            _random.Enqueue(0.99f);

            var ghost = selector.Select(huntsPlayed: 5);

            Assert.AreSame(_shade, ghost);
            Object.DestroyImmediate(lurker);
        }

        [Test]
        public void Select_NightOnlyGhostInWitchingHour_CanTurnUp()
        {
            var lurker = CreateGhost("lurker", GhostRarity.Rare, nightOnly: true);
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, _shade, lurker }, "wisp", 75f, 25f, 0f), _random,
                _witchingHour);
            _clock.LocalNow = new DateTime(2026, 10, 31, 23, 30, 0);
            _random.Enqueue(0.99f);

            var ghost = selector.Select(huntsPlayed: 5);

            Assert.AreSame(lurker, ghost);
            Object.DestroyImmediate(lurker);
        }

        [Test]
        public void Select_WitchingHour_DoublesTheLegendaryShare()
        {
            var mimic = CreateGhost("mimic", GhostRarity.Legendary);
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, _shade, mimic }, "wisp", 60f, 30f, 10f), _random,
                _witchingHour);
            _clock.LocalNow = new DateTime(2026, 11, 1, 2, 0, 0);

            var legendary = CountPicks(selector, mimic);

            Assert.AreEqual(18, legendary);
            Object.DestroyImmediate(mimic);
        }

        [Test]
        public void Select_Daytime_KeepsTheLegendaryShare()
        {
            var mimic = CreateGhost("mimic", GhostRarity.Legendary);
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, _shade, mimic }, "wisp", 60f, 30f, 10f), _random,
                _witchingHour);

            var legendary = CountPicks(selector, mimic);

            Assert.AreEqual(10, legendary);
            Object.DestroyImmediate(mimic);
        }

        [Test]
        public void Select_SeasonalGhostWhileTheEventIsOff_NeverComes()
        {
            var jack = CreateGhost("jack_o_lantern", GhostRarity.Rare, season: "halloween");
            var halloween = new HalloweenFixture(running: false);
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, jack }, "wisp", 75f, 25f, 0f), _random, _witchingHour,
                halloween.Season, DemoSwitches.Off);
            _random.DefaultValue = 0f;

            var ghost = selector.Select(huntsPlayed: 5);

            Assert.AreNotSame(jack, ghost);
            Object.DestroyImmediate(jack);
        }

        [Test]
        public void Select_EventRunningAndTheRollIsUnderThePercent_ComesTheSeasonalGhost()
        {
            var jack = CreateGhost("jack_o_lantern", GhostRarity.Rare, season: "halloween");
            var halloween = new HalloweenFixture();
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, jack }, "wisp", 75f, 25f, 0f), _random, _witchingHour,
                halloween.Season, DemoSwitches.Off);
            _random.Enqueue(34f / 100f);

            var ghost = selector.Select(huntsPlayed: 5);

            Assert.AreSame(jack, ghost);
            Object.DestroyImmediate(jack);
        }

        [Test]
        public void Select_EventRunningAndTheRollIsOverThePercent_UsesTheOrdinaryDraw()
        {
            var jack = CreateGhost("jack_o_lantern", GhostRarity.Rare, season: "halloween");
            var halloween = new HalloweenFixture();
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, jack }, "wisp", 75f, 25f, 0f), _random, _witchingHour,
                halloween.Season, DemoSwitches.Off);
            _random.DefaultValue = 0.9f;

            var ghost = selector.Select(huntsPlayed: 5);

            Assert.AreSame(_poltergeist, ghost);
            Object.DestroyImmediate(jack);
        }

        [Test]
        public void Select_SecondHuntAndTheEventGhostComesFromTheSecond_ComesTheSeasonalGhost()
        {
            var jack = CreateGhost("jack_o_lantern", GhostRarity.Rare, season: "halloween");
            var halloween = new HalloweenFixture();
            halloween.Remote.Values[RemoteKeys.HalloweenJackFromHunt] = "2";
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, _wisp, jack }, "wisp", 75f, 25f, 0f), _random, _witchingHour,
                halloween.Season, DemoSwitches.Off);
            _random.Enqueue(34f / 100f);

            var ghost = selector.Select(huntsPlayed: 1);

            Assert.AreSame(jack, ghost);
            Object.DestroyImmediate(jack);
        }

        [Test]
        public void Select_SecondHuntAndTheEventGhostComesFromTheThird_SkipsTheSeasonalGhost()
        {
            var jack = CreateGhost("jack_o_lantern", GhostRarity.Rare, season: "halloween");
            var halloween = new HalloweenFixture();
            halloween.Remote.Values[RemoteKeys.HalloweenJackFromHunt] = "3";
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, jack }, "wisp", 75f, 25f, 0f), _random, _witchingHour,
                halloween.Season, DemoSwitches.Off);
            _random.DefaultValue = 0f;

            var ghost = selector.Select(huntsPlayed: 1);

            Assert.AreNotSame(jack, ghost);
            Object.DestroyImmediate(jack);
        }

        [Test]
        public void Select_EventRunningButItIsTheFirstHunt_StaysTheTaughtGhost()
        {
            var jack = CreateGhost("jack_o_lantern", GhostRarity.Rare, season: "halloween");
            var halloween = new HalloweenFixture();
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, _wisp, jack }, "wisp", 75f, 25f, 0f), _random, _witchingHour,
                halloween.Season, DemoSwitches.Off);
            _random.DefaultValue = 0f;

            var ghost = selector.Select(huntsPlayed: 0);

            Assert.AreSame(_wisp, ghost);
            Object.DestroyImmediate(jack);
        }

        [Test]
        public void Select_DemoNamesAGhost_EveryHuntIsAgainstIt()
        {
            var jack = CreateGhost("jack_o_lantern", GhostRarity.Rare, season: "halloween");
            var demo = new DemoSwitchesBuilder().Halloween().Ghost("jack_o_lantern").Build();
            var halloween = new HalloweenFixture(demo: demo);
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, _wisp, jack }, "wisp", 75f, 25f, 0f), _random, _witchingHour,
                halloween.Season, demo);

            Assert.AreSame(jack, selector.Select(huntsPlayed: 0));
            Assert.AreSame(jack, selector.Select(huntsPlayed: 7));
            Object.DestroyImmediate(jack);
        }

        private int CountPicks(GhostSelector selector, GhostData target)
        {
            var picks = 0;
            const int samples = 100;
            for (var i = 0; i < samples; i++)
            {
                _random.Enqueue((i + 0.5f) / samples);
                if (selector.Select(huntsPlayed: 5) == target)
                    picks++;
            }

            return picks;
        }

        private static GhostData CreateGhost(string id, GhostRarity rarity, bool nightOnly = false, string season = "")
        {
            var ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(ghost);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("_nightOnly").boolValue = nightOnly;
            serialized.FindProperty("_seasonEvent").stringValue = season;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return ghost;
        }
    }
}
