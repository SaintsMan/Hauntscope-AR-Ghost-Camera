using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntAnalyticsTests
    {
        private GhostFixture _fixture;
        private GhostData _data;
        private HuntSession _session;
        private HuntLaunchOptions _options;
        private FakeAnalyticsService _analytics;
        private FakeCrashReporter _reporter;
        private HuntAnalytics _hunt;
        private FakeApplicationLifecycle _lifecycle;
        private FakeSaveService _save;
        private UnfinishedHuntLog _log;
        private PlayerProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _data = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(_data);
            serialized.FindProperty("_id").stringValue = "wisp";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _session = new HuntSession();
            _options = new HuntLaunchOptions();
            _analytics = new FakeAnalyticsService();
            _reporter = new FakeCrashReporter();
            _lifecycle = new FakeApplicationLifecycle();
            _save = new FakeSaveService();
            _log = new UnfinishedHuntLog(_save, _analytics);
            _progress = new PlayerProgress();
            var beam = TestConfigs.Beam(_session, _fixture.Camera, TestConfigs.Tools(), new HuntModifiers());
            _hunt = new HuntAnalytics(_session, _options, _analytics, _reporter, beam, _progress, _lifecycle, _log);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void Start_VirtualShift_LogsHuntStartWithEnvironmentAndMode()
        {
            _options.Select(HuntEnvironment.Virtual);
            _options.SelectMode(HuntMode.Shift);

            _hunt.Start();

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.HuntStart));
            Assert.AreEqual("virtual", _analytics.ParameterOf(AnalyticsNames.HuntStart, AnalyticsNames.Environment));
            Assert.AreEqual("shift", _analytics.ParameterOf(AnalyticsNames.HuntStart, AnalyticsNames.Mode));
        }

        [Test]
        public void Start_VirtualShift_PutsTheHuntIntoCrashReports()
        {
            _options.Select(HuntEnvironment.Virtual);
            _options.SelectMode(HuntMode.Shift);

            _hunt.Start();

            Assert.AreEqual(CrashKeys.HuntScreen, _reporter.KeyOf(CrashKeys.Screen));
            Assert.AreEqual("virtual", _reporter.KeyOf(CrashKeys.Mode));
            Assert.AreEqual("shift", _reporter.KeyOf(CrashKeys.HuntMode));
        }

        [Test]
        public void Finish_Captured_LogsHuntEndWithGhostAndRoundedDuration()
        {
            _hunt.Start();
            _session.Begin(_fixture.Ghost, _data);
            _session.AddTime(41.6f);

            _session.Finish(HuntOutcome.Captured);

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.HuntEnd));
            Assert.AreEqual("captured", _analytics.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.Outcome));
            Assert.AreEqual("wisp", _analytics.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.Ghost));
            Assert.AreEqual("42", _analytics.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.DurationSeconds));
        }

        [Test]
        public void DoubleCaptureReward_AfterFinish_DoesNotLogASecondEnd()
        {
            _hunt.Start();
            _session.Begin(_fixture.Ghost, _data);
            _session.Finish(HuntOutcome.Captured);

            _session.DoubleCaptureReward();

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.HuntEnd));
        }

        [Test]
        public void Dispose_MidHunt_LogsQuit()
        {
            _hunt.Start();
            _session.Begin(_fixture.Ghost, _data);

            _hunt.Dispose();

            Assert.AreEqual("quit", _analytics.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.Outcome));
        }

        [Test]
        public void Dispose_AfterResult_LogsNoQuit()
        {
            _hunt.Start();
            _session.Begin(_fixture.Ghost, _data);
            _session.Finish(HuntOutcome.Escaped);

            _hunt.Dispose();

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.HuntEnd));
            Assert.AreEqual("escaped", _analytics.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.Outcome));
        }

        [Test]
        public void Dispose_BeforeAnyGhost_LogsQuitScan()
        {
            _hunt.Start();

            _hunt.Dispose();

            Assert.AreEqual("quit_scan", _analytics.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.Outcome));
        }

        [Test]
        public void Start_Virtual_SetsPlayModeProperty()
        {
            _options.Select(HuntEnvironment.Virtual);

            _hunt.Start();

            Assert.AreEqual("virtual", _analytics.UserProperties[AnalyticsNames.PlayModeProperty]);
        }

        [Test]
        public void Paused_MidHunt_TheNextLaunchLogsTheQuit()
        {
            _hunt.Start();
            _session.Begin(_fixture.Ghost, _data);
            _session.AddTime(12f);

            _lifecycle.Pause();
            var nextLaunch = new FakeAnalyticsService();
            new UnfinishedHuntLog(_save, nextLaunch).Start();

            Assert.AreEqual("quit", nextLaunch.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.Outcome));
            Assert.AreEqual("yes", nextLaunch.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.Background));
            Assert.AreEqual("12", nextLaunch.ParameterOf(AnalyticsNames.HuntEnd, AnalyticsNames.DurationSeconds));
        }

        [Test]
        public void Resumed_AfterPause_TheNextLaunchLogsNothing()
        {
            _hunt.Start();
            _session.Begin(_fixture.Ghost, _data);
            _lifecycle.Pause();

            _lifecycle.Resume();
            var nextLaunch = new FakeAnalyticsService();
            new UnfinishedHuntLog(_save, nextLaunch).Start();

            Assert.AreEqual(0, nextLaunch.Count(AnalyticsNames.HuntEnd));
        }

        [Test]
        public void Finish_AfterPauseWithoutResume_ClearsTheUnfinishedHunt()
        {
            _hunt.Start();
            _session.Begin(_fixture.Ghost, _data);
            _lifecycle.Pause();

            _session.Finish(HuntOutcome.Captured);
            var nextLaunch = new FakeAnalyticsService();
            new UnfinishedHuntLog(_save, nextLaunch).Start();

            Assert.AreEqual(0, nextLaunch.Count(AnalyticsNames.HuntEnd));
        }

        [Test]
        public void Finish_SecondRoundAfterReset_LogsBothEnds()
        {
            _hunt.Start();
            _session.Begin(_fixture.Ghost, _data);
            _session.Finish(HuntOutcome.Captured);
            _session.Reset();
            _session.Begin(new GhostFixture().Ghost, _data);

            _session.Finish(HuntOutcome.Escaped);

            Assert.AreEqual(2, _analytics.Count(AnalyticsNames.HuntEnd));
        }
    }
}
