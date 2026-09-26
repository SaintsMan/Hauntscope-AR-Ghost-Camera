using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class NightmareAbilityTests
    {
        private const float LitThreshold = 0.05f;
        private const float LungeDistance = 1.3f;
        private const float Bite = 0.2f;
        private const float BounceDistance = 3f;
        private const float BounceDuration = 0.35f;
        private const float SpentStagger = 1.2f;
        private const float RetreatDistance = 3.5f;
        private const float Lit = 0.1f;
        private const float Step = 0.01f;

        private GhostFixture _fixture;
        private NightmareAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Camera.Position = new Vector3(0f, 1.5f, -1.5f);
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 1.5f));
            _fixture.Ghost.Start();
            _ability = new NightmareAbility(LitThreshold, LungeDistance, Bite, BounceDistance, BounceDuration, SpentStagger, RetreatDistance);
        }

        [Test]
        public void Tick_InTheLight_HeadsForThePlayer()
        {
            _fixture.Ghost.SetReveal(Lit);

            _ability.Tick(_fixture.Ghost, Step);

            var target = _fixture.Mover.Target;
            Assert.AreEqual(_fixture.Camera.Position.x, target.x, 1e-3f);
            Assert.AreEqual(_fixture.Camera.Position.z, target.z, 1e-3f);
        }

        [Test]
        public void Tick_InTheDark_BacksAway()
        {
            _ability.Tick(_fixture.Ghost, Step);

            var target = _fixture.Mover.Target;
            Assert.Greater(target.z, _fixture.Camera.Position.z + 1f);
        }

        [Test]
        public void Tick_LitWithinReach_LungesAndBitesTheBattery()
        {
            var lunges = 0;
            _fixture.Ghost.Lunged += () => lunges++;
            Close();

            _ability.Tick(_fixture.Ghost, Step);

            Assert.AreEqual(1, lunges);
            Assert.AreEqual(Bite, _fixture.Ghost.TakeBite());
            Assert.IsTrue(_ability.IsBouncing);
        }

        [Test]
        public void Tick_LungeOver_SpringsAwayAndIsSpent()
        {
            Close();
            _ability.Tick(_fixture.Ghost, Step);

            _ability.Tick(_fixture.Ghost, BounceDuration);

            var toPlayer = _fixture.Ghost.Position - _fixture.Camera.Position;
            toPlayer.y = 0f;
            Assert.AreEqual(BounceDistance, toPlayer.magnitude, 1e-3f);
            Assert.IsFalse(_ability.IsBouncing);
            Assert.IsTrue(_fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Tick_WithinReachButBeamed_DoesNotLunge()
        {
            Close();
            _fixture.Ghost.SetBeamed(true);

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsFalse(_ability.IsBouncing);
        }

        [Test]
        public void Tick_WithinReachButSpent_DoesNotLunge()
        {
            Close();
            _fixture.Ghost.Stagger(SpentStagger);

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsFalse(_ability.IsBouncing);
        }

        [Test]
        public void Tick_WithinReachInTheDark_DoesNotLunge()
        {
            Close();
            _fixture.Ghost.SetReveal(0f);

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsFalse(_ability.IsBouncing);
        }

        private void Close()
        {
            _fixture.Mover.Teleport(new Vector3(0f, 1f, _fixture.Camera.Position.z + LungeDistance * 0.5f));
            _fixture.Ghost.SetReveal(Lit);
        }
    }
}
