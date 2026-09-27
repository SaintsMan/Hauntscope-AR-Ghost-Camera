using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Research;
using Hauntscope.Gameplay.Story;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class TapeArchiveTests
    {
        private const int ToDeclassify = 2;

        private readonly List<StoryTape> _unlocked = new List<StoryTape>();

        private GhostData _wisp;
        private GhostData _shade;
        private PlayerProgress _progress;
        private PlayerProgressRepository _repository;
        private StoryTape _firstCatch;
        private StoryTape _threeCatches;
        private StoryTape _declassified;
        private StoryTape _shift;
        private StoryTape _twoGhosts;
        private TapeArchive _archive;

        [SetUp]
        public void SetUp()
        {
            _wisp = Ghost("wisp");
            _shade = Ghost("shade");
            _progress = new PlayerProgress();
            _repository = new PlayerProgressRepository(new FakeSaveService());
            _firstCatch = new StoryTape(1, TapeTrigger.Captures, 1, "t1", "l1", "x1");
            _threeCatches = new StoryTape(2, TapeTrigger.Captures, 3, "t2", "l2", "x2");
            _declassified = new StoryTape(3, TapeTrigger.Declassified, 1, "t3", "l3", "x3");
            _shift = new StoryTape(4, TapeTrigger.Shifts, 1, "t4", "l4", "x4");
            _twoGhosts = new StoryTape(5, TapeTrigger.DifferentGhosts, 2, "t5", "l5", "x5");
            _unlocked.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _archive?.Dispose();
            Object.DestroyImmediate(_wisp);
            Object.DestroyImmediate(_shade);
        }

        [Test]
        public void IsUnlocked_NewAgent_NothingHasSurfaced()
        {
            Start();

            Assert.AreEqual(0, _archive.UnlockedCount);
            Assert.AreEqual(0, _archive.UnheardCount);
        }

        [Test]
        public void IsUnlocked_FirstCatch_SurfacesTheFirstTapeAndAnnouncesIt()
        {
            Start();

            _progress.AddCapture("wisp", 0);

            Assert.IsTrue(_archive.IsUnlocked(_firstCatch));
            Assert.IsFalse(_archive.IsUnlocked(_threeCatches));
            CollectionAssert.AreEqual(new[] { _firstCatch }, _unlocked);
        }

        [Test]
        public void IsUnlocked_EarnedBeforeTheGameStarted_IsThereWithoutAFanfare()
        {
            _progress.AddCapture("wisp", 0);
            _progress.AddCapture("wisp", 0);
            _progress.AddCapture("wisp", 0);

            Start();

            Assert.IsTrue(_archive.IsUnlocked(_firstCatch));
            Assert.IsTrue(_archive.IsUnlocked(_threeCatches));
            Assert.AreEqual(_archive.UnlockedCount, _archive.UnheardCount);
            Assert.IsEmpty(_unlocked);
        }

        [Test]
        public void Unlocked_LaterProgressChanges_AnnouncesATapeOnlyOnce()
        {
            Start();
            _progress.AddCapture("wisp", 0);

            _progress.AddEctoplasm(5);
            _progress.MarkSighted("shade");

            Assert.AreEqual(1, _unlocked.Count);
        }

        [Test]
        public void IsUnlocked_FileDeclassified_SurfacesTheSealTape()
        {
            Start();

            _progress.AddCapture("wisp", 0);
            _progress.AddCapture("wisp", 0);

            Assert.IsTrue(_archive.IsUnlocked(_declassified));
        }

        [Test]
        public void IsUnlocked_ShiftCompleted_SurfacesTheShiftTape()
        {
            Start();

            _progress.CompleteShift();

            Assert.IsTrue(_archive.IsUnlocked(_shift));
        }

        [Test]
        public void IsUnlocked_SameGhostCaughtTwice_DoesNotCountAsTwoGhosts()
        {
            Start();

            _progress.AddCapture("wisp", 0);
            _progress.AddCapture("wisp", 0);

            Assert.IsFalse(_archive.IsUnlocked(_twoGhosts));
        }

        [Test]
        public void IsUnlocked_TwoDifferentGhostsCaught_SurfacesTheTape()
        {
            Start();

            _progress.AddCapture("wisp", 0);
            _progress.AddCapture("shade", 0);

            Assert.IsTrue(_archive.IsUnlocked(_twoGhosts));
        }

        [Test]
        public void MarkHeard_UnlockedTape_IsNoLongerNew()
        {
            Start();
            _progress.AddCapture("wisp", 0);

            _archive.MarkHeard(_firstCatch);

            Assert.IsTrue(_archive.IsHeard(_firstCatch));
            Assert.AreEqual(0, _archive.UnheardCount);
            Assert.IsTrue(_repository.Load().HasSeenTip(_firstCatch.HeardKey));
        }

        [Test]
        public void MarkHeard_LockedTape_StaysUnheard()
        {
            Start();

            _archive.MarkHeard(_threeCatches);

            Assert.IsFalse(_archive.IsHeard(_threeCatches));
        }

        [Test]
        public void MarkIntroSeen_FirstLaunch_IsRemembered()
        {
            Start();

            _archive.MarkIntroSeen();

            Assert.IsTrue(_archive.IsIntroSeen);
            Assert.IsTrue(_repository.Load().HasSeenTip("story.intro"));
        }

        [Test]
        public void IsHeard_TipsReset_EveryTapeIsNewAgain()
        {
            Start();
            _progress.AddCapture("wisp", 0);
            _archive.MarkHeard(_firstCatch);

            _progress.ResetTips();

            Assert.AreEqual(1, _archive.UnheardCount);
        }

        [Test]
        public void Dispose_AfterwardsProgress_NoLongerAnnounces()
        {
            Start();

            _archive.Dispose();
            _progress.AddCapture("wisp", 0);

            Assert.IsEmpty(_unlocked);
        }

        private void Start()
        {
            var config = new StoryConfig(new[] { "p1" }, new[] { _firstCatch, _threeCatches, _declassified, _shift, _twoGhosts });
            var ghosts = new GhostConfig(new[] { _wisp, _shade }, "wisp", 1f, 1f, 1f);
            var research = new GhostResearch(_progress, new ResearchConfig(ToDeclassify, 0f));
            _archive = new TapeArchive(_progress, _repository, research, ghosts, config);
            _archive.Unlocked += _unlocked.Add;
            _archive.Initialize();
        }

        private static GhostData Ghost(string id)
        {
            var ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(ghost);
            serialized.FindProperty("_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return ghost;
        }
    }
}
