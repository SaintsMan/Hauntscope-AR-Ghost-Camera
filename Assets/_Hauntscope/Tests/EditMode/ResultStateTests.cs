using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.States;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ResultStateTests
    {
        private GhostFixture _fixture;
        private HuntSession _session;
        private Battery _battery;
        private Toolbelt _toolbelt;
        private ResultState _state;
        private PlayerProgress _progress;
        private FakeSaveService _save;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            var config = TestConfigs.Tools();
            _battery = new Battery(config);
            _toolbelt = new Toolbelt(new GhostLens(_session, _fixture.Camera, config, new HuntModifiers()), new CaptureBeam(_session, _fixture.Camera, config, new HuntModifiers()));
            _progress = new PlayerProgress();
            _save = new FakeSaveService();
            _state = new ResultState(_session, _battery, _toolbelt, _progress, new PlayerProgressRepository(_save));
        }

        [Test]
        public void Enter_ToolsActive_TurnsThemOff()
        {
            _toolbelt.StartBeam();

            _state.Enter();

            Assert.AreEqual(0f, _toolbelt.TotalDrainPerSecond);
        }

        [Test]
        public void Exit_AfterHunt_RefillsBattery()
        {
            _battery.Drain(60f);

            _state.Exit();

            Assert.AreEqual(1f, _battery.Normalized);
        }

        [Test]
        public void Exit_AfterHunt_ResetsCaptureProgress()
        {
            _fixture.Ghost.SetReveal(1f);
            _toolbelt.Beam.Activate();
            _toolbelt.Beam.Tick(1f);
            Assume.That(_toolbelt.Beam.Progress.Value, Is.GreaterThan(0f));

            _state.Exit();

            Assert.AreEqual(0f, _toolbelt.Beam.Progress.Value);
        }

        [Test]
        public void Exit_AfterHunt_RemovesPreviousGhost()
        {
            _state.Exit();

            Assert.IsNull(_session.Ghost.Value);
            Assert.IsTrue(_fixture.View.IsDespawned);
        }

        [Test]
        public void Enter_Captured_AddsRewardAndCaptureAndSaves()
        {
            var data = CreateGhost("wisp");
            _session.Begin(_fixture.Ghost, data);
            _session.Finish(HuntOutcome.Captured);

            _state.Enter();

            Assert.AreEqual(data.Capture.Reward, _progress.Ectoplasm.Value);
            Assert.AreEqual(1, _progress.GetCaptureCount(data.Id));
            Assert.AreEqual(1, _save.SaveCount);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void Enter_Escaped_SavesWithoutReward()
        {
            var data = CreateGhost("wisp");
            _session.Begin(_fixture.Ghost, data);
            _session.Finish(HuntOutcome.Escaped);

            _state.Enter();

            Assert.AreEqual(0, _progress.Ectoplasm.Value);
            Assert.AreEqual(1, _save.SaveCount);
            Object.DestroyImmediate(data);
        }

        private static GhostData CreateGhost(string id)
        {
            var data = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        [Test]
        public void Enter_EscapedWithPickups_PaysOutTheFoundEctoplasm()
        {
            _session.Begin(_fixture.Ghost, CreateGhost("wisp"));
            _session.Finish(HuntOutcome.Escaped, false, 9);

            _state.Enter();

            Assert.AreEqual(9, _progress.Ectoplasm.Value);
            Assert.AreEqual(0, _progress.GetCaptureCount("wisp"));
        }
    }
}
