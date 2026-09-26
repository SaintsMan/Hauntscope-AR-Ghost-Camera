using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HomebodyAbilityTests
    {
        private const float IntervalMin = 1.6f;
        private const float IntervalMax = 2.4f;
        private const float FirstKnock = 0.8f;
        private const float Step = 0.01f;

        private HomebodyAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _ability = new HomebodyAbility(IntervalMin, IntervalMax, FirstKnock);
        }

        [Test]
        public void Tick_NotHiding_DoesNotKnock()
        {
            var fixture = new GhostFixture();
            fixture.Ghost.Start();
            var knocks = 0;
            fixture.Ghost.Knocked += () => knocks++;

            _ability.Tick(fixture.Ghost, 5f);

            Assert.AreEqual(0, knocks);
        }

        [Test]
        public void Tick_JustHidden_KnocksAfterTheFirstDelay()
        {
            var fixture = Hidden();
            var knocks = 0;
            fixture.Ghost.Knocked += () => knocks++;

            _ability.Tick(fixture.Ghost, FirstKnock * 0.5f);
            var early = knocks;
            _ability.Tick(fixture.Ghost, FirstKnock * 0.5f + Step);

            Assert.AreEqual(0, early);
            Assert.AreEqual(1, knocks);
        }

        [Test]
        public void Tick_StillHiding_KnocksAgainWithinTheInterval()
        {
            var fixture = Hidden();
            fixture.Random.DefaultValue = 0f;
            var knocks = 0;
            fixture.Ghost.Knocked += () => knocks++;
            _ability.Tick(fixture.Ghost, Step);
            _ability.Tick(fixture.Ghost, FirstKnock);

            _ability.Tick(fixture.Ghost, IntervalMin + Step);

            Assert.AreEqual(2, knocks);
        }

        [Test]
        public void Tick_AlertedCloseInViewAfterMinTime_RattlesOnce()
        {
            var fixture = AlertedInFront();
            var rattles = 0;
            fixture.Ghost.Rattled += () => rattles++;

            _ability.Tick(fixture.Ghost, GhostFixture.ScareMinTime);
            _ability.Tick(fixture.Ghost, Step);

            Assert.AreEqual(1, rattles);
        }

        [Test]
        public void Tick_AlertedCloseInViewTooEarly_DoesNotRattle()
        {
            var fixture = AlertedInFront();
            var rattles = 0;
            fixture.Ghost.Rattled += () => rattles++;

            _ability.Tick(fixture.Ghost, GhostFixture.ScareMinTime * 0.5f);

            Assert.AreEqual(0, rattles);
        }

        [Test]
        public void Tick_AlertedBehindThePlayer_DoesNotRattle()
        {
            var fixture = AlertedInFront();
            fixture.Camera.Forward = Vector3.back;
            var rattles = 0;
            fixture.Ghost.Rattled += () => rattles++;

            _ability.Tick(fixture.Ghost, GhostFixture.ScareMinTime);

            Assert.AreEqual(0, rattles);
        }

        private static GhostFixture Hidden()
        {
            var fixture = new GhostFixture(hide: new HideConfig(1f, 5f, 8f, 8f, 1.2f, 2f, 2f));
            fixture.HideSpots.SpotList.Add(new Vector3(1.5f, 0f, 1.5f));
            Alert(fixture.Ghost);
            fixture.Ghost.Tick(Step);
            Assert.IsTrue(fixture.Ghost.IsHiding);
            return fixture;
        }

        private static GhostFixture AlertedInFront()
        {
            var fixture = new GhostFixture();
            Alert(fixture.Ghost);
            fixture.Mover.Teleport(fixture.Camera.Position + Vector3.forward * (GhostFixture.ScareDistance * 0.5f));
            Assert.IsTrue(fixture.Ghost.IsAlerted);
            return fixture;
        }

        private static void Alert(Ghost ghost)
        {
            ghost.Start();
            ghost.SetReveal(1f);
            ghost.Tick(Step);
            ghost.SetReveal(0f);
            ghost.Tick(GhostFixture.AlertedPause + Step);
        }
    }
}
