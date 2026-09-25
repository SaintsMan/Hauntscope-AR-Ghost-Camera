using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ShriekAbilityTests
    {
        private const float TriggerDistance = 2f;
        private const float RevealThreshold = 0.6f;
        private const float JamDuration = 2f;
        private const float Cooldown = 9f;

        private GhostFixture _fixture;
        private ShriekAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1.5f, 0f));
            _ability = new ShriekAbility(TriggerDistance, RevealThreshold, JamDuration, Cooldown);
        }

        [Test]
        public void Tick_RevealedButFar_DoesNotShriek()
        {
            _fixture.Mover.Teleport(new Vector3(0f, 1.5f, 1.9f));
            _fixture.Ghost.SetReveal(1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.IsFalse(_ability.IsJamming);
            Assert.AreEqual(1f, _fixture.Ghost.Reveal);
        }

        [Test]
        public void Tick_CloseButNotRevealed_DoesNotShriek()
        {
            _fixture.Ghost.SetReveal(RevealThreshold - 0.1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.IsFalse(_ability.IsJamming);
        }

        [Test]
        public void Tick_RevealedAndClose_ShrieksAndDropsOutOfLens()
        {
            var shrieked = false;
            _fixture.Ghost.Shrieked += () => shrieked = true;
            _fixture.Ghost.SetReveal(1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.IsTrue(shrieked);
            Assert.AreEqual(0f, _fixture.Ghost.Reveal);
            Assert.IsFalse(_fixture.Ghost.IsVisible);
        }

        [Test]
        public void Tick_JamOver_VisibleAgain()
        {
            _fixture.Ghost.SetReveal(1f);
            _ability.Tick(_fixture.Ghost, 0.1f);

            _ability.Tick(_fixture.Ghost, JamDuration);

            Assert.IsTrue(_fixture.Ghost.IsVisible);
            Assert.IsFalse(_ability.IsJamming);
        }

        [Test]
        public void Tick_DuringCooldown_DoesNotShriekAgain()
        {
            _fixture.Ghost.SetReveal(1f);
            _ability.Tick(_fixture.Ghost, 0.1f);
            _ability.Tick(_fixture.Ghost, JamDuration);
            _fixture.Ghost.SetReveal(1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.IsFalse(_ability.IsJamming);
            Assert.AreEqual(1f, _fixture.Ghost.Reveal);
        }
    }
}
