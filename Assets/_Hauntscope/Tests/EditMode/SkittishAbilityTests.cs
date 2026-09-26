using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SkittishAbilityTests
    {
        private const float Step = 0.05f;
        private const float SitDistance = 1.2f;
        private const float SitDuration = 3f;

        [Test]
        public void Tick_SuddenStep_SendsTheCatRunning()
        {
            var fixture = Create();
            fixture.Ghost.Start();
            fixture.Ghost.Tick(Step);

            fixture.Camera.Position += new Vector3(0.1f, 0f, 0f);
            fixture.Ghost.Tick(Step);
            fixture.Ghost.Tick(Step);

            Assert.IsTrue(fixture.Ghost.IsFleeing);
        }

        [Test]
        public void Tick_QuickTurn_SendsTheCatRunning()
        {
            var fixture = Create();
            fixture.Ghost.Start();
            fixture.Ghost.Tick(Step);

            fixture.Camera.Forward = Quaternion.Euler(0f, 30f, 0f) * Vector3.forward;
            fixture.Ghost.Tick(Step);
            fixture.Ghost.Tick(Step);

            Assert.IsTrue(fixture.Ghost.IsFleeing);
        }

        [Test]
        public void Tick_StandingStill_ComesOverAndSitsByThePlayer()
        {
            var fixture = Create();
            var settled = false;
            fixture.Ghost.Settled += () => settled = true;
            fixture.Mover.Teleport(new Vector3(0f, 0.4f, 1.5f));
            fixture.Ghost.Start();

            for (var elapsed = 0f; elapsed < 10f && !settled; elapsed += Step)
                fixture.Ghost.Tick(Step);

            var fromPlayer = fixture.Ghost.Position - fixture.Camera.Position;
            fromPlayer.y = 0f;
            Assert.IsTrue(settled);
            Assert.IsTrue(fixture.Ghost.IsSettled);
            Assert.IsTrue(fixture.Ghost.IsStaggered);
            Assert.AreEqual(SitDistance, fromPlayer.magnitude, 0.3f);
        }

        [Test]
        public void Tick_SuddenMoveWhileSitting_EndsTheSitAndBolts()
        {
            var fixture = Create();
            var settled = false;
            fixture.Ghost.Settled += () => settled = true;
            fixture.Mover.Teleport(new Vector3(0f, 0.4f, 1.5f));
            fixture.Ghost.Start();
            for (var elapsed = 0f; elapsed < 10f && !settled; elapsed += Step)
                fixture.Ghost.Tick(Step);

            fixture.Camera.Position += new Vector3(0.1f, 0f, 0f);
            fixture.Ghost.Tick(Step);
            fixture.Ghost.Tick(Step);

            Assert.IsFalse(fixture.Ghost.IsSettled);
            Assert.IsTrue(fixture.Ghost.IsFleeing);
        }

        [Test]
        public void Tick_StillForLessThanCalmTime_KeepsWandering()
        {
            var fixture = Create();
            var settled = false;
            fixture.Ghost.Settled += () => settled = true;
            fixture.Mover.Teleport(new Vector3(0f, 0.4f, -0.3f));
            fixture.Ghost.Start();

            for (var elapsed = 0f; elapsed < 1f; elapsed += Step)
                fixture.Ghost.Tick(Step);

            Assert.IsFalse(settled);
        }

        private static GhostFixture Create()
        {
            var ability = new SkittishAbility(sharpSpeed: 0.5f, sharpTurn: 90f, calmSpeed: 0.1f, calmTurn: 15f, calmTime: 1.5f,
                sitDistance: SitDistance, sitDuration: SitDuration, arriveDistance: 0.25f, smoothing: 0.15f, sitCooldown: 4f);
            return new GhostFixture(abilities: new IGhostAbility[] { ability });
        }
    }
}
