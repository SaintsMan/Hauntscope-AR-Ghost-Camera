using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Boot;
using Hauntscope.Gameplay.Config;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hauntscope.Tests.EditMode
{
    public sealed class BootSequenceTests
    {
        private const float Intro = 1f;
        private const float StepMin = 0.5f;
        private const float Timeout = 3f;
        private const float Outro = 0.5f;
        private const string OkKey = "splash.status.ok";

        private FakeSceneLoader _sceneLoader;
        private FakeBootTask _first;
        private FakeBootTask _second;
        private BootSequence _sequence;
        private List<int> _started;
        private List<(int Index, string Status)> _completed;

        [SetUp]
        public void SetUp()
        {
            _sceneLoader = new FakeSceneLoader();
            _first = new FakeBootTask();
            _second = new FakeBootTask();
            _sequence = Create(_first, _second);
        }

        [TearDown]
        public void TearDown()
        {
            _sequence.Dispose();
        }

        [Test]
        public void Tick_BeforeIntroEnds_DoesNotRunTasks()
        {
            _sequence.Tick(Intro * 0.9f);

            Assert.AreEqual(BootPhase.Intro, _sequence.Phase.Value);
            Assert.AreEqual(0, _first.RunCount);
        }

        [Test]
        public void Tick_IntroEnds_StartsFirstStep()
        {
            _sequence.Tick(Intro);

            Assert.AreEqual(BootPhase.Steps, _sequence.Phase.Value);
            Assert.AreEqual(1, _first.RunCount);
            CollectionAssert.AreEqual(new[] { 0 }, _started);
        }

        [Test]
        public void Tick_TaskDoneBeforeMinDuration_KeepsLineUntilMinDuration()
        {
            _sequence.Tick(Intro);
            _first.Complete(OkKey);

            _sequence.Tick(StepMin * 0.5f);

            Assert.IsEmpty(_completed);
        }

        [Test]
        public void Tick_TaskDoneAndMinDurationPassed_CompletesStepAndStartsNext()
        {
            _sequence.Tick(Intro);
            _first.Complete(OkKey);

            _sequence.Tick(StepMin);

            CollectionAssert.AreEqual(new[] { (0, OkKey) }, _completed);
            Assert.AreEqual(1, _second.RunCount);
        }

        [Test]
        public void Tick_TaskStillRunning_HoldsProgressShortOfStepEnd()
        {
            _sequence.Tick(Intro);

            _sequence.Tick(StepMin);

            Assert.Less(_sequence.Progress.Value, 0.5f);
            Assert.IsEmpty(_completed);
        }

        [Test]
        public void Tick_TaskHangsPastTimeout_CompletesWithTimeoutAndCancelsTask()
        {
            _sequence.Tick(Intro);

            _sequence.Tick(Timeout);

            CollectionAssert.AreEqual(new[] { (0, BootSequence.TimeoutKey) }, _completed);
            Assert.IsTrue(_first.WasCancelled);
        }

        [Test]
        public void Tick_TaskFinishesAfterTimeout_DoesNotOverwriteNextStep()
        {
            _sequence.Tick(Intro);
            _sequence.Tick(Timeout);

            _first.Complete(OkKey);
            _sequence.Tick(StepMin);

            Assert.AreEqual(1, _completed.Count);
        }

        [Test]
        public void Tick_TaskThrows_CompletesWithFailedKey()
        {
            LogAssert.Expect(LogType.Exception, new Regex("boot task failed"));
            _sequence.Tick(Intro);
            _first.Fail();

            _sequence.Tick(StepMin);

            CollectionAssert.AreEqual(new[] { (0, BootSequence.FailedKey) }, _completed);
        }

        [Test]
        public void Tick_AllStepsDone_EntersOutroWithFullProgressWithoutLoading()
        {
            CompleteAllSteps();

            Assert.AreEqual(BootPhase.Outro, _sequence.Phase.Value);
            Assert.AreEqual(1f, _sequence.Progress.Value);
            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void Tick_OutroEnds_LoadsMainMenuOnce()
        {
            CompleteAllSteps();

            _sequence.Tick(Outro);
            _sequence.Tick(Outro);

            Assert.AreEqual(BootPhase.Done, _sequence.Phase.Value);
            Assert.AreEqual(1, _sceneLoader.LoadCount);
            Assert.AreEqual(SceneId.MainMenu, _sceneLoader.LastScene);
        }

        [Test]
        public void Tick_NoTasks_GoesFromIntroToOutro()
        {
            _sequence.Dispose();
            _sequence = Create();

            _sequence.Tick(Intro);

            Assert.AreEqual(BootPhase.Outro, _sequence.Phase.Value);
        }

        private BootSequence Create(params IBootTask[] tasks)
        {
            var sequence = new BootSequence(tasks, new BootConfig(Intro, StepMin, Timeout, Outro), _sceneLoader);
            _started = new List<int>();
            _completed = new List<(int, string)>();
            sequence.StepStarted += index => _started.Add(index);
            sequence.StepCompleted += (index, status) => _completed.Add((index, status));
            sequence.Start();
            return sequence;
        }

        private void CompleteAllSteps()
        {
            _sequence.Tick(Intro);
            _first.Complete(OkKey);
            _sequence.Tick(StepMin);
            _second.Complete(OkKey);
            _sequence.Tick(StepMin);
        }
    }
}
