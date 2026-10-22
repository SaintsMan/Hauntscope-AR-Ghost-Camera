using System.Collections.Generic;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PlayerProgressRepositoryTests
    {
        private FakeSaveService _save;
        private PlayerProgressRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _save = new FakeSaveService();
            _repository = new PlayerProgressRepository(_save);
        }

        [Test]
        public void Load_NoSave_ReturnsFreshProgress()
        {
            var progress = _repository.Load();

            Assert.AreEqual(0, progress.Ectoplasm.Value);
            Assert.IsTrue(progress.IsFirstSession);
        }

        [Test]
        public void Load_AfterSave_RestoresEverything()
        {
            var progress = new PlayerProgress();
            progress.AddCapture("wisp", 10);
            progress.AddCapture("wisp", 10);
            progress.AddCapture("shade", 30);
            progress.RegisterSession();
            progress.MarkVirtualRoomNoticeShown();
            _repository.Save(progress);

            var loaded = _repository.Load();

            Assert.AreEqual(50, loaded.Ectoplasm.Value);
            Assert.AreEqual(2, loaded.GetCaptureCount("wisp"));
            Assert.AreEqual(1, loaded.GetCaptureCount("shade"));
            Assert.AreEqual(1, loaded.TotalSessions);
            Assert.IsTrue(loaded.VirtualRoomNoticeShown);
        }

        [Test]
        public void Load_AfterSave_RestoresSeenTips()
        {
            var progress = new PlayerProgress();
            progress.MarkTipSeen("tip.stagger");

            _repository.Save(progress);

            Assert.IsTrue(_repository.Load().HasSeenTip("tip.stagger"));
        }

        [Test]
        public void Load_AfterSave_RestoresEvpEvidence()
        {
            var progress = new PlayerProgress();
            progress.AddEvpEvidence("banshee");

            _repository.Save(progress);

            Assert.IsTrue(_repository.Load().HasEvpEvidence("banshee"));
        }

        [Test]
        public void Load_AfterSave_RestoresUnlockedAndUsedFeatures()
        {
            var progress = new PlayerProgress();
            progress.UnlockFeature("shop");
            progress.MarkFeatureUsed("contracts");
            progress.MarkFeaturesInitialized();

            _repository.Save(progress);
            var loaded = _repository.Load();

            Assert.IsTrue(loaded.IsFeatureUnlocked("shop"));
            Assert.IsFalse(loaded.IsFeatureUnlocked("archive"));
            Assert.IsFalse(loaded.MarkFeatureUsed("contracts"));
            Assert.IsTrue(loaded.FeaturesInitialized);
        }

        [Test]
        public void Load_SaveWithoutFeatureFields_StartsUninitialized()
        {
            _save.Save("player_progress", new PlayerProgressDto(PlayerProgressRepository.CurrentVersion, 5, new List<CaptureCountDto>(), 2, false,
                false, 0, new List<string>(), 0, 0, new List<CaptureCountDto>(), 0, 0, new List<string>(), new List<string>()));

            var loaded = _repository.Load();

            Assert.IsFalse(loaded.FeaturesInitialized);
            Assert.AreEqual(2, loaded.TotalSessions);
            Assert.IsFalse(loaded.IsFeatureUnlocked("shop"));
        }

        [Test]
        public void ResetTips_AfterFeaturesOpened_KeepsThemOpen()
        {
            var progress = new PlayerProgress();
            progress.UnlockFeature("shop");
            progress.MarkTipSeen("coach.shop");

            progress.ResetTips();

            Assert.IsTrue(progress.IsFeatureUnlocked("shop"));
        }

        [Test]
        public void Save_Always_WritesCurrentVersion()
        {
            _repository.Save(new PlayerProgress());

            Assert.IsTrue(_save.TryLoad<PlayerProgressDto>("player_progress", out var dto));
            Assert.AreEqual(PlayerProgressRepository.CurrentVersion, dto.Version);
        }

        [Test]
        public void Load_NewerVersion_StartsFresh()
        {
            _save.SetRaw("player_progress", "{\"_version\":99,\"_ectoplasm\":500}");

            var progress = _repository.Load();

            Assert.AreEqual(0, progress.Ectoplasm.Value);
        }

        [Test]
        public void Load_MissingVersion_StartsFresh()
        {
            _save.SetRaw("player_progress", "{\"_ectoplasm\":500}");

            var progress = _repository.Load();

            Assert.AreEqual(0, progress.Ectoplasm.Value);
        }

        [Test]
        public void Load_AfterReviewPrompt_RestoresPromptedCaptureCount()
        {
            var progress = new PlayerProgress();
            progress.AddCapture("wisp", 10);
            progress.MarkReviewPrompted();
            _repository.Save(progress);

            var loaded = _repository.Load();

            Assert.AreEqual(1, loaded.ReviewPromptedAtCaptures);
        }

        [Test]
        public void Load_SaveWithoutReviewField_ReadsNeverPrompted()
        {
            _save.SetRaw("player_progress", "{\"_version\":1,\"_ectoplasm\":40}");

            var progress = _repository.Load();

            Assert.AreEqual(0, progress.ReviewPromptedAtCaptures);
        }

        [Test]
        public void Load_AfterSightings_RestoresSightedGhosts()
        {
            var progress = new PlayerProgress();
            progress.MarkSighted("banshee");
            _repository.Save(progress);

            var loaded = _repository.Load();

            Assert.IsTrue(loaded.IsSighted("banshee"));
            Assert.IsFalse(loaded.IsSighted("mimic"));
        }

        [Test]
        public void Load_AfterSpending_KeepsTheRestOfTheBalance()
        {
            var progress = new PlayerProgress();
            progress.AddEctoplasm(100);
            progress.TrySpend(30);
            _repository.Save(progress);

            Assert.AreEqual(70, _repository.Load().Ectoplasm.Value);
        }
    }
}
