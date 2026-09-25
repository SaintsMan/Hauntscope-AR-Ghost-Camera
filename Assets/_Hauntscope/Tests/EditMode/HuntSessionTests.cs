using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntSessionTests
    {
        private GhostFixture _fixture;
        private GhostData _data;
        private HuntSession _session;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _data = ScriptableObject.CreateInstance<GhostData>();
            _session = new HuntSession();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void Begin_Always_ExposesGhostAndData()
        {
            _session.Begin(_fixture.Ghost, _data);

            Assert.AreSame(_fixture.Ghost, _session.Ghost.Value);
            Assert.AreSame(_data, _session.GhostData);
        }

        [Test]
        public void Finish_AfterTime_RecordsOutcomeAndDuration()
        {
            _session.Begin(_fixture.Ghost, _data);
            _session.AddTime(12.5f);

            _session.Finish(HuntOutcome.Captured);

            Assert.AreEqual(HuntOutcome.Captured, _session.Result.Value.Outcome);
            Assert.AreEqual(12.5f, _session.Result.Value.Duration);
            Assert.AreSame(_data, _session.Result.Value.Ghost);
        }

        [Test]
        public void Result_Captured_GrantsGhostReward()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Captured);

            Assert.AreEqual(_data.Capture.Reward, _session.Result.Value.Reward);
        }

        [Test]
        public void Result_Escaped_GrantsNothing()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Escaped);

            Assert.AreEqual(0, _session.Result.Value.Reward);
        }

        [Test]
        public void RequestHuntAgain_NoResultYet_IsIgnored()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.RequestHuntAgain();

            Assert.IsFalse(_session.IsHuntAgainRequested);
        }

        [Test]
        public void RequestHuntAgain_AfterResult_IsRequested()
        {
            _session.Begin(_fixture.Ghost, _data);
            _session.Finish(HuntOutcome.Escaped);

            _session.RequestHuntAgain();

            Assert.IsTrue(_session.IsHuntAgainRequested);
        }

        [Test]
        public void Reset_AfterHunt_DespawnsGhostAndClearsEverything()
        {
            _session.Begin(_fixture.Ghost, _data);
            _session.AddTime(5f);
            _session.Finish(HuntOutcome.Captured);
            _session.RequestHuntAgain();

            _session.Reset();

            Assert.IsTrue(_fixture.View.IsDespawned);
            Assert.IsNull(_session.Ghost.Value);
            Assert.IsNull(_session.Result.Value);
            Assert.AreEqual(0f, _session.Elapsed);
            Assert.IsFalse(_session.IsHuntAgainRequested);
        }
    }
}
