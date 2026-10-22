using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Ghosts.Abilities;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PumpkinPatchAbilityTests
    {
        private const int DecoysMin = 4;
        private const int DecoysMax = 5;
        private const float MinSpacing = 0.6f;
        private const float MinCameraDistance = 0.5f;
        private const float RevealIn = 0.4f;
        private const float RevealOut = 0.8f;
        private const float DudDuration = 0.9f;
        private const float SpentBite = 0.03f;
        private const float WakeStagger = 0.9f;
        private const float Step = 0.1f;

        private FakeLensSight _lens;
        private List<FakeGhostView> _views;
        private PumpkinPatchAbility _ability;
        private GhostFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _lens = new FakeLensSight();
            _views = new List<FakeGhostView>();
            _ability = NewAbility();
            _fixture = NewFixture(_ability, new SweepRandom());
        }

        [Test]
        public void Tick_FirstTick_PlantsTheDecoysAndTheRadarHearsThemAll()
        {
            _fixture.Ghost.Tick(Step);

            Assert.AreEqual(DecoysMin, _views.Count);
            Assert.AreEqual(_views.Count + 1, _fixture.Ghost.EmfSources.Count);
        }

        [Test]
        public void Tick_Disguised_KeepsTheGhostWhereItSits()
        {
            _fixture.Ghost.Tick(Step);
            var start = _fixture.Ghost.Position;

            for (var i = 0; i < 40; i++)
                _fixture.Ghost.Tick(Step);

            Assert.Less(Vector3.Distance(start, _fixture.Ghost.Position), 0.3f);
            Assert.IsFalse(_ability.IsAwake);
        }

        [Test]
        public void Tick_LensOnADecoy_SpendsItAndTakesABiteOfBattery()
        {
            _fixture.Ghost.Tick(Step);
            var spent = 0;
            _fixture.Ghost.DecoySpent += _ => spent++;
            var target = _fixture.Ghost.EmfSources[1];
            _lens.Looking = point => point == target;

            RunFor(RevealIn + Step);

            Assert.AreEqual(1, spent);
            Assert.AreEqual(SpentBite, _fixture.Ghost.TakeBite(), 1e-5f);
        }

        [Test]
        public void Tick_DecoySpent_LeavesTheRadar()
        {
            _fixture.Ghost.Tick(Step);
            var before = _fixture.Ghost.EmfSources.Count;
            var target = _fixture.Ghost.EmfSources[1];
            _lens.Looking = point => point == target;

            RunFor(RevealIn + Step);

            Assert.AreEqual(before - 1, _fixture.Ghost.EmfSources.Count);
        }

        [Test]
        public void Tick_LensGlancesOffADecoy_LetsItFadeBackWithoutSpendingIt()
        {
            _fixture.Ghost.Tick(Step);
            var spent = 0;
            _fixture.Ghost.DecoySpent += _ => spent++;
            var target = _fixture.Ghost.EmfSources[1];
            _lens.Looking = point => point == target;
            _fixture.Ghost.Tick(Step);
            _lens.Looking = _ => false;

            RunFor(RevealOut * 2f);

            Assert.AreEqual(0, spent);
            Assert.AreEqual(0f, _views[0].Reveal, 1e-5f);
            Assert.AreEqual(DecoysMin, _ability.DecoysLeft);
        }

        [Test]
        public void Tick_SpentDecoyBurnsOut_IsTakenAway()
        {
            _fixture.Ghost.Tick(Step);
            var target = _fixture.Ghost.EmfSources[1];
            _lens.Looking = point => point == target;
            RunFor(RevealIn + Step);
            _lens.Looking = _ => false;

            RunFor(DudDuration + Step);

            Assert.AreEqual(1, _views.FindAll(view => view.IsDespawned).Count);
        }

        [Test]
        public void Tick_GhostRevealedByTheLens_WakesAndTheRadarHearsOnlyIt()
        {
            _fixture.Ghost.Tick(Step);
            var awakened = 0;
            _fixture.Ghost.Awakened += () => awakened++;
            _fixture.Ghost.SetReveal(_fixture.Config.AlertRevealThreshold + 0.1f);

            _fixture.Ghost.Tick(Step);

            Assert.AreEqual(1, awakened);
            Assert.IsTrue(_ability.IsAwake);
            Assert.AreEqual(1, _fixture.Ghost.EmfSources.Count);
            Assert.IsTrue(_fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Tick_Awake_TheOtherPumpkinsWindDown()
        {
            _fixture.Ghost.Tick(Step);
            _fixture.Ghost.SetReveal(_fixture.Config.AlertRevealThreshold + 0.1f);
            _fixture.Ghost.Tick(Step);

            RunFor(DudDuration + Step);

            Assert.AreEqual(0, _ability.DecoysLeft);
            Assert.IsTrue(_views.TrueForAll(view => view.IsDespawned));
        }

        [Test]
        public void Tick_AfterWaking_TheGhostMovesAgain()
        {
            _fixture.Ghost.Tick(Step);
            _fixture.Ghost.SetReveal(_fixture.Config.AlertRevealThreshold + 0.1f);
            _fixture.Ghost.Tick(Step);
            _fixture.Ghost.SetReveal(0f);
            RunFor(8f);
            var settled = _fixture.Ghost.Position;

            RunFor(8f);

            Assert.Greater(Vector3.Distance(settled, _fixture.Ghost.Position), 0.05f);
        }

        [Test]
        public void Capture_WhileDisguised_StopsTheRadarHearingThePatch()
        {
            _fixture.Ghost.Tick(Step);

            _fixture.Ghost.Capture();

            Assert.AreEqual(1, _fixture.Ghost.EmfSources.Count);
        }

        private PumpkinPatchAbility NewAbility()
        {
            return new PumpkinPatchAbility(DecoysMin, DecoysMax, MinSpacing, MinCameraDistance, RevealIn, RevealOut, DudDuration, SpentBite,
                WakeStagger, position =>
                {
                    var view = new FakeGhostView();
                    _views.Add(view);
                    return view;
                });
        }

        private GhostFixture NewFixture(PumpkinPatchAbility ability, IRandom random)
        {
            var fixture = new GhostFixture(abilities: new IGhostAbility[] { ability }, lens: _lens, contextRandom: random);
            fixture.Mover.Teleport(new Vector3(0f, 1f, 1f));
            fixture.Ghost.Start();
            return fixture;
        }

        private void RunFor(float seconds)
        {
            for (var elapsed = 0f; elapsed < seconds; elapsed += Step)
                _fixture.Ghost.Tick(Step);
        }

        // A repeatable stream that never repeats a value soon, so the pumpkins land in different places. The integer
        // range answers its lower bound: the patch is always the smallest it may be.
        private sealed class SweepRandom : IRandom
        {
            private const float Golden = 0.61803398f;
            private float _state = 0.17f;

            public float Value => Next();

            public float Range(float minInclusive, float maxInclusive)
            {
                return Mathf.Lerp(minInclusive, maxInclusive, Next());
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                return minInclusive;
            }

            private float Next()
            {
                _state = (_state + Golden) % 1f;
                return _state;
            }
        }
    }
}
