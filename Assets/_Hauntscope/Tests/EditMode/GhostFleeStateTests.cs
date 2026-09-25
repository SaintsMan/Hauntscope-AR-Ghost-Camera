using Hauntscope.Gameplay.Ghosts.States;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostFleeStateTests
    {
        private GhostFixture _fixture;
        private bool _beamed;
        private GhostFleeState _state;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Camera.Position = new Vector3(0f, 1.5f, -1.5f);
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            _beamed = true;
            _state = new GhostFleeState(_fixture.Context, () => _beamed);
        }

        [Test]
        public void Enter_Always_TargetsAwayFromCamera()
        {
            _state.Enter();

            Assert.AreEqual(new Vector3(0f, 1f, GhostFixture.FleeDistance), _fixture.Mover.Target);
        }

        [Test]
        public void Tick_Always_IncreasesDistanceToCamera()
        {
            _state.Enter();
            var before = Vector3.Distance(_fixture.Mover.Position, _fixture.Camera.Position);

            _state.Tick(0.2f);

            Assert.Greater(Vector3.Distance(_fixture.Mover.Position, _fixture.Camera.Position), before);
        }

        [Test]
        public void IsCalm_StillBeamed_IsFalse()
        {
            _state.Enter();

            _state.Tick(GhostFixture.FleeCalmDownTime + 1f);

            Assert.IsFalse(_state.IsCalm);
        }

        [Test]
        public void IsCalm_NoBeamForCalmDownTime_IsTrue()
        {
            _state.Enter();
            _beamed = false;

            _state.Tick(GhostFixture.FleeCalmDownTime);

            Assert.IsTrue(_state.IsCalm);
        }

        [Test]
        public void IsCalm_BeamReturnsBeforeCalm_ResetsTimer()
        {
            _state.Enter();
            _beamed = false;
            _state.Tick(GhostFixture.FleeCalmDownTime - 0.5f);
            _beamed = true;
            _state.Tick(0.1f);
            _beamed = false;

            _state.Tick(0.5f);

            Assert.IsFalse(_state.IsCalm);
        }
    }
}
