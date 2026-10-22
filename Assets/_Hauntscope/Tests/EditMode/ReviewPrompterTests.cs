using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Photo;
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
        private MenuPopupQueue _popups;
        private PhotoAlbum _album;
        private FakeAnalyticsService _analytics;

        [SetUp]
        public void SetUp()
        {
            _review = new FakeInAppReview();
            _save = new FakeSaveService();
            _repository = new PlayerProgressRepository(_save);
            _progress = new PlayerProgress();
            var config = new ReviewConfig(MinCaptures, 10, 0f, 3);
            _popups = new MenuPopupQueue();
            _album = new PhotoAlbum(new FakePhotoStorage(), TestConfigs.Photo());
            _analytics = new FakeAnalyticsService();
            _prompter = new ReviewPrompter(new ReviewPolicy(config, new RemoteTuning(new FakeRemoteValues())), config, _progress, _repository,
                _review, _popups, _album, _analytics);
        }

        [TearDown]
        public void TearDown()
        {
            _prompter.Dispose();
        }

        [Test]
        public void Start_ThreeStarPhotoBeforeAnyCatch_AsksAndLogsThePhoto()
        {
            _album.Add(new PhotoRecord("a.png", "wisp", 3, 0));

            _prompter.Start();

            Assert.AreEqual("photo", _analytics.ParameterOf(AnalyticsNames.ReviewPrompt, AnalyticsNames.Reason));
            Assert.AreEqual(1, _progress.ReviewPromptedAtCaptures);
        }

        [Test]
        public void Start_TwoStarPhoto_DoesNotAsk()
        {
            _album.Add(new PhotoRecord("a.png", "wisp", 2, 0));

            _prompter.Start();

            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.ReviewPrompt));
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
        public void Start_MenuCardOpen_WaitsForIt()
        {
            CaptureMinimum();
            _popups.Enqueue(new HeldPopup());

            _prompter.Start();

            Assert.AreEqual(0, _review.RequestCount);
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
    
        private sealed class HeldPopup : IMenuPopup
        {
            public void Open()
            {
            }

            public void Close()
            {
            }
        }
    }
}
