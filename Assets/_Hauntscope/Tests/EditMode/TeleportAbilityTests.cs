using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class TeleportAbilityTests
    {
        private const float Threshold = 0.4f;
        private const float Cooldown = 5f;

        private GhostFixture _fixture;
        private TeleportAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Planes.RoomBounds = new Bounds(Vector3.zero, new Vector3(10f, 0f, 10f));
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            _ability = new TeleportAbility(Threshold, 2f, 3f, Cooldown);
        }

        [Test]
        public void Tick_ProgressBelowThreshold_StaysInPlace()
        {
            _fixture.Ghost.SetCaptureProgress(Threshold - 0.1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.AreEqual(new Vector3(0f, 1f, 0f), _fixture.Ghost.Position);
        }

        [Test]
        public void Tick_ProgressAboveThreshold_TeleportsWithinDistanceRange()
        {
            _fixture.Ghost.SetCaptureProgress(Threshold + 0.1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            var distance = Vector3.Distance(new Vector3(0f, 1f, 0f), _fixture.Ghost.Position);
            Assert.That(distance, Is.InRange(2f, 3f));
        }

        [Test]
        public void Tick_Teleported_KeepsHeight()
        {
            _fixture.Ghost.SetCaptureProgress(1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.AreEqual(1f, _fixture.Ghost.Position.y);
        }

        [Test]
        public void Tick_DuringCooldown_DoesNotTeleportAgain()
        {
            _fixture.Ghost.SetCaptureProgress(1f);
            _ability.Tick(_fixture.Ghost, 0.1f);
            var afterFirst = _fixture.Ghost.Position;

            _ability.Tick(_fixture.Ghost, Cooldown - 1f);

            Assert.AreEqual(afterFirst, _fixture.Ghost.Position);
        }

        [Test]
        public void Tick_CooldownOver_TeleportsAgain()
        {
            _fixture.Ghost.SetCaptureProgress(1f);
            _ability.Tick(_fixture.Ghost, 0.1f);
            var afterFirst = _fixture.Ghost.Position;
            _ability.Tick(_fixture.Ghost, Cooldown);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.AreNotEqual(afterFirst, _fixture.Ghost.Position);
        }

        [Test]
        public void Tick_TargetOutsideRoom_StaysInsideRoomBounds()
        {
            _fixture.Planes.RoomBounds = new Bounds(Vector3.zero, new Vector3(3f, 0f, 3f));
            _fixture.Ghost.SetCaptureProgress(1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            var position = _fixture.Ghost.Position;
            Assert.That(Mathf.Abs(position.x), Is.LessThanOrEqualTo(1.5f));
            Assert.That(Mathf.Abs(position.z), Is.LessThanOrEqualTo(1.5f));
        }
    }
}
