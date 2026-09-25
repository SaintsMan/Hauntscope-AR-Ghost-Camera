using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ReviewPrompterTests
    {
        private const int MinCaptures = 2;

        private FakeInAppReview _review;
        private FakeSaveService _save;
        private PlayerProgressRepository _repository;
        private PlayerProgress _progress;
        private ReviewPrompter _prompter;

        [SetUp]
        public void SetUp()
        {
            _review = new FakeInAppReview();
            _save = new FakeSaveService();
            _repository = new PlayerProgressRepository(_save);
            _progress = new PlayerProgress();
            var config = new ReviewConfig(MinCaptures, 10, 0f);
            _prompter = new ReviewPrompter(new ReviewPolicy(config), config, _progress, _repository, _review);
        }

        [TearDown]
        public void TearDown()
        {
            _prompter.Dispose();
        }

        [Test]
        public void Start_PolicyDeclines_DoesNotRequestOrSave()
        {
            _prompter.Start();

            Assert.AreEqual(0, _review.RequestCount);
            Assert.AreEqual(0, _save.SaveCount);
        }

        [Test]
        public void Start_PolicyAccepts_RequestsReview()
        {
            CaptureMinimum();

            _prompter.Start();

            Assert.AreEqual(1, _review.RequestCount);
        }

        [Test]
        public void Start_PolicyAccepts_SavesPromptBeforeAsking()
        {
            CaptureMinimum();

            _prompter.Start();

            Assert.AreEqual(MinCaptures, _repository.Load().ReviewPromptedAtCaptures);
        }

        private void CaptureMinimum()
        {
            for (var i = 0; i < MinCaptures; i++)
                _progress.AddCapture("wisp", 10);
        }
    }
}
