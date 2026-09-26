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
        public void Finish_FirstCapture_MarksNewEntry()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Captured, true);

            Assert.IsTrue(_session.Result.Value.IsFirstCapture);
        }

        [Test]
        public void Finish_EscapedFlaggedFirst_IsNotNewEntry()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Escaped, true);

            Assert.IsFalse(_session.Result.Value.IsFirstCapture);
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

        [Test]
        public void Finish_CapturedWithPickups_RewardAddsFound()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Captured, false, 7);

            var result = _session.Result.Value;
            Assert.AreEqual(_data.Capture.Reward, result.CaptureReward);
            Assert.AreEqual(7, result.Found);
            Assert.AreEqual(_data.Capture.Reward + 7, result.Reward);
        }

        [Test]
        public void Finish_EscapedWithPickups_KeepsOnlyFound()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Escaped, false, 5);

            Assert.AreEqual(0, _session.Result.Value.CaptureReward);
            Assert.AreEqual(5, _session.Result.Value.Reward);
        }

        [Test]
        public void Finish_ResearchMultiplier_RoundsTheCaptureBonus()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Captured, false, 0, 1.25f);

            Assert.AreEqual(Mathf.RoundToInt(_data.Capture.Reward * 1.25f), _session.Result.Value.CaptureReward);
        }

        [Test]
        public void DoubleCaptureReward_Captured_DoublesOnlyTheCatch()
        {
            _session.Begin(_fixture.Ghost, _data);
            _session.Finish(HuntOutcome.Captured, false, 6);

            _session.DoubleCaptureReward();

            var result = _session.Result.Value;
            Assert.IsTrue(result.IsDoubled);
            Assert.AreEqual(_data.Capture.Reward * 2 + 6, result.Reward);
        }

        [Test]
        public void DoubleCaptureReward_Escaped_ChangesNothing()
        {
            _session.Begin(_fixture.Ghost, _data);
            _session.Finish(HuntOutcome.Escaped, false, 6);

            _session.DoubleCaptureReward();

            Assert.IsFalse(_session.Result.Value.IsDoubled);
            Assert.AreEqual(6, _session.Result.Value.Reward);
        }

        [Test]
        public void Begin_NewHunt_ForgetsThePreviousSighting()
        {
            _session.Begin(_fixture.Ghost, _data);
            _session.MarkSighted();

            _session.Begin(_fixture.Ghost, _data);

            Assert.IsFalse(_session.IsSighted);
        }

        [Test]
        public void Finish_DeclassifyingCatch_IsFlagged()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Captured, false, 0, 1f, true);

            Assert.IsTrue(_session.Result.Value.IsDeclassified);
        }

        [Test]
        public void Finish_EscapedEvenIfFlagged_IsNotDeclassified()
        {
            _session.Begin(_fixture.Ghost, _data);

            _session.Finish(HuntOutcome.Escaped, false, 0, 1f, true);

            Assert.IsFalse(_session.Result.Value.IsDeclassified);
        }
    }
}
