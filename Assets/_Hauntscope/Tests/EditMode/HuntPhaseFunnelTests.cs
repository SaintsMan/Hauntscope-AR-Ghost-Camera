using System;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntPhaseFunnelTests
    {
        private GhostFixture _fixture;
        private GhostData _data;
        private HuntSession _session;
        private FakePlaneProvider _planes;
        private RoomCalibration _calibration;
        private PlayerProgress _progress;
        private FakeAnalyticsService _analytics;
        private FakeClock _clock;
        private HuntPhaseFunnel _funnel;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _data = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(_data);
            serialized.FindProperty("_id").stringValue = "wisp";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _session = new HuntSession();
            _planes = new FakePlaneProvider();
            _calibration = new RoomCalibration(_planes, new RoomConfig(2f, 1.5f, 0.3f, 2f, 5f));
            _progress = new PlayerProgress();
            _analytics = new FakeAnalyticsService();
            _clock = new FakeClock();
            var radar = new EmfRadar(_fixture.Camera, new FakeRandom(), new EmfConfig(5, 0.6f, 0f, 0.2f, 1.2f, 0.1f, 1f, 1.3f, 4));
            var beam = TestConfigs.Beam(_session, _fixture.Camera, TestConfigs.Tools(), new HuntModifiers());
            _funnel = new HuntPhaseFunnel(_session, _calibration, radar, beam, _progress, new OnboardingConfig(), new HuntLaunchOptions(),
                _analytics, _clock);
        }

        [TearDown]
        public void TearDown()
        {
            _funnel.Dispose();
            UnityEngine.Object.DestroyImmediate(_data);
        }

        [Test]
        public void Calibration_Completes_ReportsRoomReadyWithSeconds()
        {
            _funnel.Start();
            _clock.Advance(TimeSpan.FromSeconds(7));

            _planes.HorizontalArea = 3f;
            _calibration.Refresh();

            Assert.AreEqual(HuntPhaseFunnel.RoomReady, _analytics.ParameterOf(AnalyticsNames.HuntPhase, AnalyticsNames.Step));
            Assert.AreEqual("7", _analytics.ParameterOf(AnalyticsNames.HuntPhase, AnalyticsNames.Seconds));
        }

        [Test]
        public void GhostAndSighting_FirstHunt_ReportsBothSteps()
        {
            _funnel.Start();

            _session.Begin(_fixture.Ghost, _data);
            _session.MarkSighted();

            Assert.AreEqual(2, _analytics.Count(AnalyticsNames.HuntPhase));
        }

        [Test]
        public void MarkSighted_Twice_ReportsOnce()
        {
            _funnel.Start();
            _session.Begin(_fixture.Ghost, _data);

            _session.MarkSighted();
            _session.MarkSighted();

            Assert.AreEqual(2, _analytics.Count(AnalyticsNames.HuntPhase));
        }

        [Test]
        public void Start_VeteranHunt_ReportsNothing()
        {
            for (var i = 0; i < 5; i++)
                _progress.RegisterSession();
            _funnel.Start();

            _session.Begin(_fixture.Ghost, _data);

            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.HuntPhase));
        }
    }
}
