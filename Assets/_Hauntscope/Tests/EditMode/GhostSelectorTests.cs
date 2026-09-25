using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostSelectorTests
    {
        private GhostData _wisp;
        private GhostData _poltergeist;
        private GhostData _shade;
        private FakeRandom _random;
        private GhostSelector _selector;

        [SetUp]
        public void SetUp()
        {
            _wisp = CreateGhost("wisp", GhostRarity.Common);
            _poltergeist = CreateGhost("poltergeist", GhostRarity.Common);
            _shade = CreateGhost("shade", GhostRarity.Rare);
            _random = new FakeRandom();
            var config = new GhostConfig(new[] { _poltergeist, _wisp, _shade }, "wisp", 75f, 25f, 0f);
            _selector = new GhostSelector(config, _random);
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
            var rare = 0;
            const int samples = 100;
            for (var i = 0; i < samples; i++)
            {
                _random.Enqueue((i + 0.5f) / samples);
                if (_selector.Select(isFirstSession: false) == _shade)
                    rare++;
            }

            Assert.AreEqual(25, rare);
        }

        [Test]
        public void Select_FirstGhostMissing_FallsBackToWeightedRandom()
        {
            var config = new GhostConfig(new[] { _shade }, "wisp", 75f, 25f, 0f);
            var selector = new GhostSelector(config, _random);

            Assert.AreSame(_shade, selector.Select(isFirstSession: true));
        }

        private static GhostData CreateGhost(string id, GhostRarity rarity)
        {
            var ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(ghost);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return ghost;
        }
    }
}
