using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Engagement
{
    // Runs when the main menu opens. Captures only happen in a hunt, so the first menu after the capture that
    // crosses the threshold is the moment right after a success, and it never interrupts a hunt in progress.
    public sealed class ReviewPrompter : IStartable, IDisposable
    {
        private readonly ReviewPolicy _policy;
        private readonly ReviewConfig _config;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly IInAppReview _review;
        private readonly MenuPopupQueue _popups;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public ReviewPrompter(
            ReviewPolicy policy,
            ReviewConfig config,
            PlayerProgress progress,
            PlayerProgressRepository repository,
            IInAppReview review,
            MenuPopupQueue popups)
        {
            _popups = popups;
            _policy = policy;
            _config = config;
            _progress = progress;
            _repository = repository;
            _review = review;
        }

        public void Start()
        {
            if (!_policy.ShouldPrompt(_progress))
                return;

            // Marked before asking: Play never reports whether the dialog was shown, so one attempt counts as the ask.
            _progress.MarkReviewPrompted();
            _repository.Save(_progress);
            RequestAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private async UniTaskVoid RequestAsync(CancellationToken cancellationToken)
        {
            if (_config.MenuDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(_config.MenuDelay), cancellationToken: cancellationToken);
            // Last in line: the review dialog never covers the menu's own cards.
            if (!_popups.IsIdle)
                await UniTask.WaitUntil(() => _popups.IsIdle, cancellationToken: cancellationToken);

            await _review.RequestAsync(cancellationToken);
        }
    }
}
