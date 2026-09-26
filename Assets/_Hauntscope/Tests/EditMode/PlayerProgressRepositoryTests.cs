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
