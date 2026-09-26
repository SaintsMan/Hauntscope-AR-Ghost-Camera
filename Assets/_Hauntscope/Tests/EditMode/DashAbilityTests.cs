using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class DashAbilityTests
    {
        private const float Distance = 1.5f;
        private const float Duration = 0.2f;
        private const float Cooldown = 2f;

        private GhostFixture _fixture;
        private DashAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            _ability = new DashAbility(Distance, Duration, Cooldown);
        }

        [Test]
        public void Tick_NotBeamed_StaysInPlace()
        {
            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.AreEqual(new Vector3(0f, 1f, 0f), _fixture.Ghost.Position);
            Assert.IsFalse(_ability.IsDashing);
        }

        [Test]
        public void Tick_Beamed_DashesAcrossLineOfSightByDistance()
        {
            _fixture.Ghost.SetBeamed(true);

            _ability.Tick(_fixture.Ghost, 0.01f);
            _ability.Tick(_fixture.Ghost, Duration);

            var position = _fixture.Ghost.Position;
            Assert.AreEqual(Distance, Mathf.Abs(position.x), 0.001f);
            Assert.AreEqual(0f, position.z, 0.001f);
            Assert.AreEqual(1f, position.y, 0.001f);
        }

        [Test]
        public void Tick_Beamed_RaisesDashedWithTarget()
        {
            _fixture.Ghost.SetBeamed(true);
            var target = Vector3.zero;
            _fixture.Ghost.Dashed += (from, to) => target = to;

            _ability.Tick(_fixture.Ghost, 0.01f);

            Assert.AreEqual(Distance, Mathf.Abs(target.x), 0.001f);
        }

        [Test]
        public void Tick_SideTowardsWall_DashesToOtherSide()
        {
            _fixture.Mover.Teleport(new Vector3(1.8f, 1f, 0f));
            _fixture.Random.Enqueue(0.9f);
            _fixture.Ghost.SetBeamed(true);

            _ability.Tick(_fixture.Ghost, 0.01f);
            _ability.Tick(_fixture.Ghost, Duration);

            Assert.Less(_fixture.Ghost.Position.x, 1.8f);
        }

        [Test]
        public void Tick_DuringCooldown_DoesNotDashAgain()
        {
            _fixture.Ghost.SetBeamed(true);
            _ability.Tick(_fixture.Ghost, 0.01f);
            _ability.Tick(_fixture.Ghost, Duration);
            var afterFirst = _fixture.Ghost.Position;

            _ability.Tick(_fixture.Ghost, Cooldown * 0.5f);

            Assert.AreEqual(afterFirst, _fixture.Ghost.Position);
            Assert.IsFalse(_ability.IsDashing);
        }

        [Test]
        public void Tick_CooldownOverAndBeamed_DashesAgain()
        {
            _fixture.Ghost.SetBeamed(true);
            _ability.Tick(_fixture.Ghost, 0.01f);
            _ability.Tick(_fixture.Ghost, Duration);
            _ability.Tick(_fixture.Ghost, Cooldown);

            _ability.Tick(_fixture.Ghost, 0.01f);

            Assert.IsTrue(_ability.IsDashing);
        }

        [Test]
        public void Tick_DashFinished_StaggersGhost()
        {
            var ability = new DashAbility(Distance, Duration, Cooldown, 0.7f);
            _fixture.Ghost.SetBeamed(true);
            ability.Tick(_fixture.Ghost, 0.01f);
            Assert.IsFalse(_fixture.Ghost.IsStaggered);

            ability.Tick(_fixture.Ghost, Duration);

            Assert.IsTrue(_fixture.Ghost.IsStaggered);
        }
    }
}
