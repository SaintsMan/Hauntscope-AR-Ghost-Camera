using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class WatchedAbilityTests
    {
        private const float Step = 0.05f;
        private const float StaggerDuration = 1.2f;
        private const float BounceDistance = 3f;
        private const float BounceDuration = 0.35f;

        [Test]
        public void Tick_LookedAt_HoldsStill()
        {
            var fixture = Create();
            fixture.Mover.Teleport(new Vector3(0f, 1.5f, 1f));
            fixture.Ghost.Start();
            fixture.Ghost.Tick(Step);
            var held = fixture.Ghost.Position;

            fixture.Ghost.Tick(1f);

            Assert.AreEqual(held, fixture.Ghost.Position);
        }

        [Test]
        public void Tick_LookedAway_CreepsTowardsThePlayersBack()
        {
            var fixture = Create();
            fixture.Mover.Teleport(new Vector3(1f, 1.5f, 1f));
            fixture.Camera.Forward = Vector3.back;
            fixture.Ghost.Start();

            fixture.Ghost.Tick(Step);

            var target = fixture.Mover.Target;
            Assert.AreEqual(0f, target.x, 1e-4f);
            Assert.AreEqual(fixture.Camera.Position.z + 1.5f, target.z, 1e-4f);
        }

        [Test]
        public void Tick_TurnedOnRightAfterItSetOff_CatchesItWinded()
        {
            var fixture = Create();
            fixture.Mover.Teleport(new Vector3(0f, 1.5f, 1f));
            fixture.Ghost.Start();
            fixture.Ghost.Tick(Step);
            fixture.Camera.Forward = Vector3.back;
            fixture.Ghost.Tick(0.2f);

            fixture.Camera.Forward = Vector3.forward;
            fixture.Ghost.Tick(Step);

            Assert.IsTrue(fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Tick_TurnedOnLongAfterItSetOff_DoesNotStagger()
        {
            var fixture = Create();
            fixture.Mover.Teleport(new Vector3(0f, 1.5f, 1f));
            fixture.Ghost.Start();
            fixture.Ghost.Tick(Step);
            fixture.Camera.Forward = Vector3.back;
            fixture.Ghost.Tick(1f);

            fixture.Camera.Forward = Vector3.forward;
            fixture.Ghost.Tick(Step);

            Assert.IsFalse(fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Tick_SetsOffAgainAfterBeingSeen_Creaks()
        {
            var fixture = Create();
            var creaks = 0;
            fixture.Ghost.Crept += () => creaks++;
            fixture.Mover.Teleport(new Vector3(0f, 1.5f, 1f));
            fixture.Camera.Forward = Vector3.back;
            fixture.Ghost.Start();
            fixture.Ghost.Tick(Step);
            fixture.Camera.Forward = Vector3.forward;
            fixture.Ghost.Tick(Step);

            fixture.Camera.Forward = Vector3.back;
            fixture.Ghost.Tick(Step);

            Assert.AreEqual(1, creaks);
        }

        [Test]
        public void Tick_UnnoticedAtThePlayersBack_LungesAndSpringsAway()
        {
            var fixture = Create(behindDistance: 0.2f);
            var lunged = false;
            fixture.Ghost.Lunged += () => lunged = true;
            fixture.Camera.Forward = Vector3.back;
            fixture.Mover.Teleport(new Vector3(0f, 1.5f, fixture.Camera.Position.z + 0.5f));
            fixture.Ghost.Start();

            for (var elapsed = 0f; elapsed < 2f && !lunged; elapsed += Step)
                fixture.Ghost.Tick(Step);
            for (var tick = 0; tick < Mathf.CeilToInt(BounceDuration / Step); tick++)
                fixture.Ghost.Tick(Step);

            var away = fixture.Ghost.Position - fixture.Camera.Position;
            away.y = 0f;
            Assert.IsTrue(lunged);
            Assert.AreEqual(BounceDistance, away.magnitude, 0.05f);
        }

        private static GhostFixture Create(float behindDistance = 1.5f)
        {
            var ability = new WatchedAbility(viewAngle: 50f, behindDistance: behindDistance, closeInSpeed: 0.15f, lungeDistance: 0.9f,
                lungeDelay: 1f, bounceDistance: BounceDistance, bounceDuration: BounceDuration, catchWindow: 0.5f, catchCooldown: 3f,
                staggerDuration: StaggerDuration, creakInterval: 2f);
            return new GhostFixture(abilities: new IGhostAbility[] { ability });
        }
    }
}
