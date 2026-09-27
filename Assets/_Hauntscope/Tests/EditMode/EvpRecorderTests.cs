using System.Collections.Generic;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class EvpRecorderTests
    {
        private const float RecordTime = 4f;
        private const float PlaybackTime = 2f;
        private const float Cooldown = 8f;
        private const float Range = 4f;
        private const float Cost = 0.03f;

        private GhostFixture _fixture;
        private HuntSession _session;
        private Battery _battery;
        private PlayerProgress _progress;
        private EvpRecorder _recorder;
        private List<EvpTake> _takes;

        [SetUp]
        public void SetUp()
        {
            _takes = new List<EvpTake>();
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
            PlaceGhost(10f);
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            _battery = new Battery(TestConfigs.Tools());
            _progress = new PlayerProgress();
            _progress.AddCapture("wisp", 10);
            _recorder = TestConfigs.EvpTool(_session, _fixture.Camera, _battery, _progress,
                TestConfigs.Evp(unlockCaptures: 1, cost: Cost, recordTime: RecordTime, playbackTime: PlaybackTime, cooldown: Cooldown, range: Range));
            _recorder.Played += take => _takes.Add(take);
            _recorder.Tick(0f);
        }

        [Test]
        public void Activate_NotEnoughCaptures_StaysReady()
        {
            var recorder = TestConfigs.EvpTool(_session, _fixture.Camera, _battery, new PlayerProgress(), TestConfigs.Evp(unlockCaptures: 9));

            recorder.Activate();

            Assert.AreEqual(EvpPhase.Ready, recorder.Phase.Value);
        }

        [Test]
        public void Activate_Unlocked_RecordsAndSpendsItsShareOfBattery()
        {
            _recorder.Activate();

            Assert.AreEqual(EvpPhase.Recording, _recorder.Phase.Value);
            Assert.IsTrue(_recorder.IsActive.Value);
            Assert.AreEqual(1f - Cost, _battery.Normalized, 1e-5f);
        }

        [Test]
        public void Activate_BatteryDepleted_StaysReady()
        {
            _battery.Drain(1000f);

            _recorder.Activate();

            Assert.AreEqual(EvpPhase.Ready, _recorder.Phase.Value);
        }

        [Test]
        public void Activate_WhileRecording_SpendsNothingMore()
        {
            _recorder.Activate();

            _recorder.Activate();

            Assert.AreEqual(1f - Cost, _battery.Normalized, 1e-5f);
        }

        [Test]
        public void Tick_GhostWithinRangeDuringTake_PlaysItsVoice()
        {
            PlaceGhost(2f);
            _recorder.Activate();

            _recorder.Tick(RecordTime + 0.01f);

            Assert.AreEqual(1, _takes.Count);
            Assert.IsTrue(_takes[0].HasVoice);
            Assert.AreEqual(EvpPhase.Playback, _recorder.Phase.Value);
        }

        [Test]
        public void Tick_GhostOutOfRangeWholeTake_PlaysStatic()
        {
            _recorder.Activate();

            _recorder.Tick(RecordTime + 0.01f);

            Assert.AreEqual(1, _takes.Count);
            Assert.IsFalse(_takes[0].HasVoice);
        }

        [Test]
        public void Tick_GhostPassedCloseMidTake_StillOnTape()
        {
            _recorder.Activate();
            _recorder.Tick(1f);
            PlaceGhost(1f);
            _recorder.Tick(1f);
            PlaceGhost(10f);

            _recorder.Tick(RecordTime);

            Assert.IsTrue(_takes[0].HasVoice);
        }

        [Test]
        public void Tick_BeforeTakeEnds_PlaysNothing()
        {
            PlaceGhost(2f);
            _recorder.Activate();

            _recorder.Tick(RecordTime * 0.5f);

            Assert.AreEqual(0, _takes.Count);
            Assert.AreEqual(RecordTime * 0.5f, _recorder.TimeLeft, 1e-4f);
        }

        [Test]
        public void Tick_VoiceOnTape_CountsAsEvidence()
        {
            PlaceGhost(2f);
            _recorder.Activate();

            _recorder.Tick(RecordTime + 0.01f);

            Assert.IsTrue(_recorder.HasEvidence);
        }

        [Test]
        public void Tick_StaticOnly_NoEvidence()
        {
            _recorder.Activate();

            _recorder.Tick(RecordTime + 0.01f);

            Assert.IsFalse(_recorder.HasEvidence);
        }

        [Test]
        public void Tick_PlaybackOver_RestsBeforeTheNextTake()
        {
            _recorder.Activate();
            _recorder.Tick(RecordTime + 0.01f);

            _recorder.Tick(PlaybackTime + 0.01f);

            Assert.AreEqual(EvpPhase.Cooldown, _recorder.Phase.Value);
            Assert.IsFalse(_recorder.IsActive.Value);
            Assert.AreEqual(0f, _recorder.Readiness, 1e-3f);
        }

        [Test]
        public void Activate_WhileResting_Ignored()
        {
            FinishTake();

            _recorder.Activate();

            Assert.AreEqual(EvpPhase.Cooldown, _recorder.Phase.Value);
            Assert.AreEqual(1f - Cost, _battery.Normalized, 1e-5f);
        }

        [Test]
        public void Tick_RestOver_ReadyAgain()
        {
            FinishTake();

            _recorder.Tick(Cooldown + 0.01f);

            Assert.AreEqual(EvpPhase.Ready, _recorder.Phase.Value);
            Assert.AreEqual(1f, _recorder.Readiness);
        }

        [Test]
        public void Tick_HalfwayThroughRest_HalfReady()
        {
            FinishTake();

            _recorder.Tick(Cooldown * 0.5f);

            Assert.AreEqual(0.5f, _recorder.Readiness, 1e-3f);
        }

        [Test]
        public void Deactivate_WhileRecording_RestsWithoutPlayingBack()
        {
            PlaceGhost(2f);
            _recorder.Activate();

            _recorder.Deactivate();
            _recorder.Tick(RecordTime + 0.01f);

            Assert.AreEqual(0, _takes.Count);
            Assert.AreEqual(EvpPhase.Cooldown, _recorder.Phase.Value);
            Assert.IsFalse(_recorder.HasEvidence);
        }

        [Test]
        public void Deactivate_WhenReady_StaysReady()
        {
            _recorder.Deactivate();

            Assert.AreEqual(EvpPhase.Ready, _recorder.Phase.Value);
        }

        [Test]
        public void Tick_NextGhost_ReadyWithNoEvidence()
        {
            PlaceGhost(2f);
            FinishTake();
            var next = new GhostFixture();
            next.Ghost.Start();
            _session.Begin(next.Ghost, null);

            _recorder.Tick(0.1f);

            Assert.AreEqual(EvpPhase.Ready, _recorder.Phase.Value);
            Assert.IsFalse(_recorder.HasEvidence);
        }

        [Test]
        public void Tick_GhostCapturedBeforeTake_NotOnTape()
        {
            PlaceGhost(2f);
            _fixture.Ghost.Capture();
            _fixture.Ghost.Tick(0.01f);
            _recorder.Activate();

            _recorder.Tick(RecordTime + 0.01f);

            Assert.IsFalse(_takes[0].HasVoice);
        }

        [Test]
        public void DrainPerSecond_Always_Zero()
        {
            _recorder.Activate();

            Assert.AreEqual(0f, _recorder.DrainPerSecond);
        }

        private void PlaceGhost(float distance)
        {
            _fixture.Camera.Position = _fixture.Ghost.Position - Vector3.forward * distance;
        }

        private void FinishTake()
        {
            _recorder.Activate();
            _recorder.Tick(RecordTime + 0.01f);
            _recorder.Tick(PlaybackTime + 0.01f);
        }
    }
}
