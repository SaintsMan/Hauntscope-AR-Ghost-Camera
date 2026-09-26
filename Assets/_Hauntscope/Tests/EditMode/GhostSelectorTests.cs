using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
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
            _witchingHour = new WitchingHour(_clock, new NightConfig(23, 4, 2f, 1.25f));
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

            var ghost = _selector.Select(isFirstSession: true);

            Assert.AreSame(_wisp, ghost);
        }

        [Test]
        public void Select_RollInFirstCommonShare_ReturnsFirstCommon()
        {
            _random.Enqueue(0.1f);

            Assert.AreSame(_poltergeist, _selector.Select(isFirstSession: false));
        }

        [Test]
        public void Select_RollInSecondCommonShare_ReturnsSecondCommon()
        {
            _random.Enqueue(0.5f);

            Assert.AreSame(_wisp, _selector.Select(isFirstSession: false));
        }

        [Test]
        public void Select_RollInRareShare_ReturnsRare()
        {
            _random.Enqueue(0.8f);

            Assert.AreSame(_shade, _selector.Select(isFirstSession: false));
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

            Assert.AreSame(_shade, selector.Select(isFirstSession: true));
        }

        [Test]
        public void Select_NightOnlyGhostByDay_TakesNoneOfTheRareShare()
        {
            var lurker = CreateGhost("lurker", GhostRarity.Rare, nightOnly: true);
            var selector = new GhostSelector(new GhostConfig(new[] { _poltergeist, _shade, lurker }, "wisp", 75f, 25f, 0f), _random,
                _witchingHour);
            _random.Enqueue(0.99f);

            var ghost = selector.Select(isFirstSession: false);

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

            var ghost = selector.Select(isFirstSession: false);

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

        private int CountPicks(GhostSelector selector, GhostData target)
        {
            var picks = 0;
            const int samples = 100;
            for (var i = 0; i < samples; i++)
            {
                _random.Enqueue((i + 0.5f) / samples);
                if (selector.Select(isFirstSession: false) == target)
                    picks++;
            }

            return picks;
        }

        private static GhostData CreateGhost(string id, GhostRarity rarity, bool nightOnly = false)
        {
            var ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(ghost);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("_nightOnly").boolValue = nightOnly;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return ghost;
        }
    }
}
