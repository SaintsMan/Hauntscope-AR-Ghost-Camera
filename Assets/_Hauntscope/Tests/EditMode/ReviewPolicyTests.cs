using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ReviewPolicyTests
    {
        private const int MinCaptures = 3;
        private const int RepeatEvery = 10;

        private ReviewPolicy _policy;
        private PlayerProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _policy = new ReviewPolicy(new ReviewConfig(MinCaptures, RepeatEvery, 0f));
            _progress = new PlayerProgress();
        }

        [Test]
        public void ShouldPrompt_FewerCapturesThanMinimum_ReturnsFalse()
        {
            Capture(MinCaptures - 1);

            Assert.IsFalse(_policy.ShouldPrompt(_progress));
        }

        [Test]
        public void ShouldPrompt_MinimumReachedNeverPrompted_ReturnsTrue()
        {
            Capture(MinCaptures);

            Assert.IsTrue(_policy.ShouldPrompt(_progress));
        }

        [Test]
        public void ShouldPrompt_JustPrompted_ReturnsFalse()
        {
            Capture(MinCaptures);
            _progress.MarkReviewPrompted();

            Assert.IsFalse(_policy.ShouldPrompt(_progress));
        }

        [Test]
        public void ShouldPrompt_FewerNewCapturesThanRepeat_ReturnsFalse()
        {
            Capture(MinCaptures);
            _progress.MarkReviewPrompted();

            Capture(RepeatEvery - 1);

            Assert.IsFalse(_policy.ShouldPrompt(_progress));
        }

        [Test]
        public void ShouldPrompt_RepeatCapturesSinceLastPrompt_ReturnsTrue()
        {
            Capture(MinCaptures);
            _progress.MarkReviewPrompted();

            Capture(RepeatEvery);

            Assert.IsTrue(_policy.ShouldPrompt(_progress));
        }

        private void Capture(int count)
        {
            for (var i = 0; i < count; i++)
                _progress.AddCapture(i % 2 == 0 ? "wisp" : "shade", 10);
        }
    }
}
