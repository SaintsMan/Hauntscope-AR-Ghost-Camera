using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class TutorialFlowTests
    {
        private const float WalkDistance = 1f;
        private const int EmfLevel = 3;
        private const float RevealThreshold = 0.5f;
        private const float BeamProgress = 0.25f;

        private GhostFixture _fixture;
        private HuntSession _session;
        private FakeCameraPose _camera;
        private EmfRadar _radar;
        private HuntPause _pause;
        private HuntLaunchOptions _options;
        private FakeSaveService _save;
        private PlayerProgressRepository _repository;
        private PlayerProgress _progress;
        private TutorialFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
            _session = new HuntSession();
            _camera = new FakeCameraPose { Position = new Vector3(0f, 1.5f, 0f), Forward = Vector3.forward };
            _radar = new EmfRadar(_camera, new FakeRandom(), new EmfConfig(5, 0.6f, 0f, 0.2f, 1.2f, 0.1f, 1f, 1.3f, 4));
            _pause = new HuntPause(new FakeTrackingStatus(), new FakeApplicationLifecycle(), new TrackingConfig(0.5f), new FakeAdsService());
            _options = new HuntLaunchOptions();
            _save = new FakeSaveService();
            _repository = new PlayerProgressRepository(_save);
            CreateFlow(new PlayerProgress());
        }

        [TearDown]
        public void TearDown()
        {
            _flow.Dispose();
        }

        [Test]
        public void GhostSpawned_Ar_StartsAtFollowEmf()
        {
            _session.Begin(_fixture.Ghost, null);

            Assert.AreEqual(TutorialStep.FollowEmf, _flow.Step.Value);
            Assert.AreEqual(1, _flow.StepNumber);
            Assert.AreEqual(3, _flow.StepCount);
        }

        [Test]
        public void GhostSpawned_Virtual_StartsAtWalk()
        {
            _options.Select(HuntEnvironment.Virtual);

            _session.Begin(_fixture.Ghost, null);

            Assert.AreEqual(TutorialStep.Walk, _flow.Step.Value);
            Assert.AreEqual(4, _flow.StepCount);
        }

        [Test]
        public void GhostSpawned_TutorialAlreadyCompleted_StaysHidden()
        {
            _flow.Dispose();
            CreateFlow(new PlayerProgress(0, new Dictionary<string, int>(), 1, false, true));

            _session.Begin(_fixture.Ghost, null);

            Assert.AreEqual(TutorialStep.None, _flow.Step.Value);
        }

        [Test]
        public void Tick_WalkedFarEnough_MovesToFollowEmf()
        {
            _options.Select(HuntEnvironment.Virtual);
            _session.Begin(_fixture.Ghost, null);
            _camera.Position += new Vector3(WalkDistance, 0f, 0f);

            _flow.Tick();

            Assert.AreEqual(TutorialStep.FollowEmf, _flow.Step.Value);
        }

        [Test]
        public void Tick_OnlyLookedAround_StaysOnWalk()
        {
            _options.Select(HuntEnvironment.Virtual);
            _session.Begin(_fixture.Ghost, null);
            _camera.Forward = Vector3.right;

            _flow.Tick();

            Assert.AreEqual(TutorialStep.Walk, _flow.Step.Value);
        }

        [Test]
        public void Tick_EmfHigh_MovesToUseLens()
        {
            _session.Begin(_fixture.Ghost, null);
            _radar.Tick(0.1f, _camera.Position + Vector3.forward * 0.5f, 6f);

            _flow.Tick();

            Assert.AreEqual(TutorialStep.UseLens, _flow.Step.Value);
        }

        [Test]
        public void Tick_EmfLow_StaysOnFollowEmf()
        {
            _session.Begin(_fixture.Ghost, null);
            _radar.Tick(0.1f, _camera.Position + Vector3.forward * 5.5f, 6f);

            _flow.Tick();

            Assert.AreEqual(TutorialStep.FollowEmf, _flow.Step.Value);
        }

        [Test]
        public void Tick_GhostRevealed_MovesToHoldBeam()
        {
            _session.Begin(_fixture.Ghost, null);
            _radar.Tick(0.1f, _camera.Position, 6f);
            _flow.Tick();
            _fixture.Ghost.SetReveal(RevealThreshold);

            _flow.Tick();

            Assert.AreEqual(TutorialStep.HoldBeam, _flow.Step.Value);
        }

        [Test]
        public void Tick_BeamProgress_CompletesAndSaves()
        {
            _session.Begin(_fixture.Ghost, null);
            _radar.Tick(0.1f, _camera.Position, 6f);
            _flow.Tick();
            _fixture.Ghost.SetReveal(1f);
            _flow.Tick();
            _fixture.Ghost.SetCaptureProgress(BeamProgress);

            _flow.Tick();

            Assert.AreEqual(TutorialStep.Done, _flow.Step.Value);
            Assert.IsTrue(_progress.TutorialCompleted);
            Assert.IsTrue(_repository.Load().TutorialCompleted);
        }

        [Test]
        public void Tick_Paused_DoesNotAdvance()
        {
            _session.Begin(_fixture.Ghost, null);
            _radar.Tick(0.1f, _camera.Position, 6f);
            _pause.PauseManually();

            _flow.Tick();

            Assert.AreEqual(TutorialStep.FollowEmf, _flow.Step.Value);
        }

        [Test]
        public void HuntFinishedEarly_CompletesTutorial()
        {
            _session.Begin(_fixture.Ghost, null);

            _session.Finish(HuntOutcome.Escaped);

            Assert.AreEqual(TutorialStep.Done, _flow.Step.Value);
            Assert.IsTrue(_progress.TutorialCompleted);
        }

        [Test]
        public void Dispose_ThenGhostSpawned_IsIgnored()
        {
            _flow.Dispose();

            _session.Begin(_fixture.Ghost, null);

            Assert.AreEqual(TutorialStep.None, _flow.Step.Value);
        }

        private void CreateFlow(PlayerProgress progress)
        {
            _progress = progress;
            _flow = new TutorialFlow(_session, _radar, _camera, _pause, _options, _progress, _repository,
                new TutorialConfig(WalkDistance, EmfLevel, RevealThreshold, BeamProgress));
            _flow.Start();
        }
    }
}
