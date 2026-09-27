using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SpiritBoxTests
    {
        private const float FirstDelay = 1f;
        private const float Interval = 4f;
        private const float Drain = 0.4f;

        private GhostFixture _fixture;
        private HuntSession _session;
        private FakeCameraPose _camera;
        private PlayerProgress _progress;
        private SpiritBox _box;
        private int _answers;
        private int _crackles;
        private SpiritBoxAnswer _lastAnswer;

        [SetUp]
        public void SetUp()
        {
            _answers = 0;
            _crackles = 0;
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
            _fixture.Mover.Teleport(new Vector3(2f, 1f, 0f));
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            _camera = new FakeCameraPose { Position = new Vector3(0f, 1.5f, 0f), Forward = Vector3.forward };
            _progress = new PlayerProgress();
            _progress.AddCapture("wisp", 10);
            var config = TestConfigs.SpiritBox(unlockCaptures: 1, drain: Drain, firstAnswerDelay: FirstDelay, intervalMin: Interval,
                intervalMax: Interval, range: 9f);
            _box = TestConfigs.SpiritBoxTool(_session, _camera, TestConfigs.Tools(beamRevealThreshold: 0.5f), new FakeRandom(), _progress, config);
            _box.Answered += answer =>
            {
                _answers++;
                _lastAnswer = answer;
            };
            _box.Crackled += () => _crackles++;
        }

        [Test]
        public void Activate_NoCapturesYet_StaysOff()
        {
            var box = TestConfigs.SpiritBoxTool(_session, _camera, TestConfigs.Tools(), new FakeRandom(), new PlayerProgress(),
                TestConfigs.SpiritBox(unlockCaptures: 1));

            box.Activate();

            Assert.IsFalse(box.IsActive.Value);
        }

        [Test]
        public void Activate_EnoughCaptures_TurnsOn()
        {
            _box.Activate();

            Assert.IsTrue(_box.IsActive.Value);
        }

        [Test]
        public void DrainPerSecond_Always_ComesFromConfig()
        {
            Assert.AreEqual(Drain, _box.DrainPerSecond);
        }

        [Test]
        public void Tick_BeforeFirstAnswerDelay_SaysNothing()
        {
            _box.Activate();

            _box.Tick(FirstDelay * 0.5f);

            Assert.AreEqual(0, _answers);
        }

        [Test]
        public void Tick_AfterFirstAnswerDelay_SaysWhereTheGhostIs()
        {
            _box.Activate();

            _box.Tick(FirstDelay);

            Assert.AreEqual(1, _answers);
            Assert.AreEqual(SpiritDirection.Right, _lastAnswer.Direction);
        }

        [Test]
        public void Tick_JustAfterAnAnswer_WaitsForTheInterval()
        {
            _box.Activate();
            _box.Tick(FirstDelay);

            _box.Tick(Interval * 0.5f);

            Assert.AreEqual(1, _answers);
        }

        [Test]
        public void Tick_IntervalPassed_AnswersAgain()
        {
            _box.Activate();
            _box.Tick(FirstDelay);

            _box.Tick(Interval);

            Assert.AreEqual(2, _answers);
        }

        [Test]
        public void Tick_BoxOff_NeverAnswers()
        {
            _box.Tick(FirstDelay + Interval);

            Assert.AreEqual(0, _answers);
        }

        [Test]
        public void Tick_GhostAlreadyRevealed_KeepsQuiet()
        {
            _fixture.Ghost.SetReveal(1f);
            _box.Activate();

            _box.Tick(FirstDelay);

            Assert.AreEqual(0, _answers);
        }

        [Test]
        public void Tick_GhostOutOfRange_OnlyCrackles()
        {
            var box = TestConfigs.SpiritBoxTool(_session, _camera, TestConfigs.Tools(), new FakeRandom(), _progress,
                TestConfigs.SpiritBox(firstAnswerDelay: FirstDelay, range: 1f));
            var answers = 0;
            var crackles = 0;
            box.Answered += _ => answers++;
            box.Crackled += () => crackles++;
            box.Activate();

            box.Tick(FirstDelay);

            Assert.AreEqual(0, answers);
            Assert.AreEqual(1, crackles);
        }

        [Test]
        public void Tick_MimicDecoyOnTheLeft_PointsAtTheRealGhost()
        {
            _fixture.Ghost.SetEmfDecoy(new Vector3(-3f, 1f, 0f));
            _box.Activate();

            _box.Tick(FirstDelay);

            Assert.AreEqual(SpiritDirection.Right, _lastAnswer.Direction);
        }

        [Test]
        public void Tick_GhostCaptured_SaysNothing()
        {
            _fixture.Ghost.Capture();
            _box.Activate();

            _box.Tick(FirstDelay);

            Assert.AreEqual(0, _answers);
            Assert.AreEqual(0, _crackles);
        }
    }
}
