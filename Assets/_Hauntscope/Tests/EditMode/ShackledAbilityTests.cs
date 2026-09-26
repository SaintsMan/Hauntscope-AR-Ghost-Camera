using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ShackledAbilityTests
    {
        private const float First = 0.4f;
        private const float Second = 0.7f;
        private const float Distance = 0.6f;
        private const float Duration = 0.3f;
        private const float GripLoss = 0.25f;
        private const float Rearm = 0.15f;
        private const float Step = 0.01f;

        private static readonly Vector3 Start = new Vector3(0f, 1f, 0f);

        private GhostFixture _fixture;
        private ShackledAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(Start);
            _fixture.Ghost.Start();
            _ability = new ShackledAbility(new[] { First, Second }, Distance, Duration, GripLoss, Rearm);
        }

        [Test]
        public void Tick_BeamedBelowTheFirstPoint_DoesNotYank()
        {
            _fixture.Ghost.SetBeamed(true);
            _fixture.Ghost.SetCaptureProgress(First - Step);

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsFalse(_ability.IsYanking);
        }

        [Test]
        public void Tick_NotBeamedPastThePoint_DoesNotYank()
        {
            _fixture.Ghost.SetCaptureProgress(First);

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsFalse(_ability.IsYanking);
        }

        [Test]
        public void Tick_BeamedAtThePoint_YanksSidewaysByDistance()
        {
            var dashes = 0;
            _fixture.Ghost.Dashed += (from, to) => dashes++;
            Beam(First);

            _ability.Tick(_fixture.Ghost, Step);
            _ability.Tick(_fixture.Ghost, Duration);

            Assert.AreEqual(1, dashes);
            Assert.AreEqual(Distance, Mathf.Abs(_fixture.Ghost.Position.x - Start.x), 1e-3f);
            Assert.AreEqual(Start.z, _fixture.Ghost.Position.z, 1e-3f);
        }

        [Test]
        public void Tick_YankEnds_TestsTheGrip()
        {
            Beam(First);
            _ability.Tick(_fixture.Ghost, Step);

            _ability.Tick(_fixture.Ghost, Duration);

            Assert.IsFalse(_ability.IsYanking);
            Assert.AreEqual(GripLoss, _fixture.Ghost.TakeGripTest());
        }

        [Test]
        public void Tick_PointAlreadyUsed_DoesNotYankAgain()
        {
            Yank(First);

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsFalse(_ability.IsYanking);
        }

        [Test]
        public void Tick_SecondPoint_YanksAgain()
        {
            Yank(First);
            Beam(Second);

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsTrue(_ability.IsYanking);
        }

        [Test]
        public void Tick_SlippedWellBelowAUsedPoint_YanksThereAgain()
        {
            Yank(First);
            _fixture.Ghost.SetCaptureProgress(First - Rearm - Step);
            _ability.Tick(_fixture.Ghost, Step);
            Beam(First);

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsTrue(_ability.IsYanking);
        }

        private void Beam(float progress)
        {
            _fixture.Ghost.SetBeamed(true);
            _fixture.Ghost.SetCaptureProgress(progress);
        }

        private void Yank(float progress)
        {
            Beam(progress);
            _ability.Tick(_fixture.Ghost, Step);
            _ability.Tick(_fixture.Ghost, Duration);
            _fixture.Ghost.TakeGripTest();
        }
    }
}
