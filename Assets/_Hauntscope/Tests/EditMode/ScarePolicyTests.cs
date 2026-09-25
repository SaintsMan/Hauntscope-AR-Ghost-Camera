using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ScarePolicyTests
    {
        private GhostFixture _fixture;
        private GameSettings _settings;
        private HuntSession _session;
        private ScarePolicy _policy;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1.5f, -0.5f));
            _fixture.Ghost.Start();
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Tick(0.01f);
            _settings = new GameSettings();
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            _session.AddTime(GhostFixture.ScareMinTime);
            _policy = new ScarePolicy(_settings, _fixture.Scare);
        }

        [Test]
        public void CanScare_AllConditionsMet_ReturnsTrue()
        {
            var result = _policy.CanScare(_session, _fixture.Ghost, _fixture.Camera);

            Assert.IsTrue(result);
        }

        [Test]
        public void CanScare_JumpScaresDisabled_ReturnsFalse()
        {
            _settings.SetJumpScares(false);

            var result = _policy.CanScare(_session, _fixture.Ghost, _fixture.Camera);

            Assert.IsFalse(result);
        }

        [Test]
        public void CanScare_PlayersFirstHunt_ReturnsFalse()
        {
            _session.Begin(_fixture.Ghost, null, true);
            _session.AddTime(GhostFixture.ScareMinTime);

            var result = _policy.CanScare(_session, _fixture.Ghost, _fixture.Camera);

            Assert.IsFalse(result);
        }

        [Test]
        public void CanScare_AlreadyScaredThisHunt_ReturnsFalse()
        {
            _session.MarkScared();

            var result = _policy.CanScare(_session, _fixture.Ghost, _fixture.Camera);

            Assert.IsFalse(result);
        }

        [Test]
        public void CanScare_BeforeMinTime_ReturnsFalse()
        {
            _session.Begin(_fixture.Ghost, null);
            _session.AddTime(GhostFixture.ScareMinTime - 1f);

            var result = _policy.CanScare(_session, _fixture.Ghost, _fixture.Camera);

            Assert.IsFalse(result);
        }

        [Test]
        public void CanScare_GhostNotAlerted_ReturnsFalse()
        {
            var calm = new GhostFixture();
            calm.Mover.Teleport(new Vector3(0f, 1.5f, -0.5f));
            calm.Ghost.Start();

            var result = _policy.CanScare(_session, calm.Ghost, _fixture.Camera);

            Assert.IsFalse(result);
        }

        [Test]
        public void CanScare_GhostTooFar_ReturnsFalse()
        {
            _fixture.Mover.Teleport(new Vector3(0f, 1.5f, GhostFixture.ScareDistance));

            var result = _policy.CanScare(_session, _fixture.Ghost, _fixture.Camera);

            Assert.IsFalse(result);
        }

        [Test]
        public void CanScare_GhostOutsideViewAngle_ReturnsFalse()
        {
            _fixture.Mover.Teleport(new Vector3(1f, 1.5f, -1.4f));

            var result = _policy.CanScare(_session, _fixture.Ghost, _fixture.Camera);

            Assert.IsFalse(result);
        }
    }
}
