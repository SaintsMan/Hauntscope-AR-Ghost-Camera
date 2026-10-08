using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Iap;

namespace Hauntscope.Gameplay.Ads
{
    // Decorator for the full version: no ad breaks, and every "watch a video" reward is granted without the video.
    // Placements keep asking the same IAdsService and never learn the player paid.
    public sealed class PremiumAdsService : IAdsService, IDisposable
    {
        private readonly IAdsService _inner;
        private readonly PurchaseHistory _history;

        public PremiumAdsService(IAdsService inner, PurchaseHistory history)
        {
            _inner = inner;
            _history = history;
            _inner.AvailabilityChanged += OnAvailabilityChanged;
            _history.Changed += OnAvailabilityChanged;
        }

        public event Action AvailabilityChanged;

        public bool IsRewardedReady => _history.IsPremium || _inner.IsRewardedReady;

        public bool IsInterstitialReady => !_history.IsPremium && _inner.IsInterstitialReady;

        public bool IsShowing => _inner.IsShowing;

        public UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            return _inner.InitializeAsync(cancellationToken);
        }

        public UniTask<bool> ShowRewardedAsync(CancellationToken cancellationToken)
        {
            return _history.IsPremium ? UniTask.FromResult(true) : _inner.ShowRewardedAsync(cancellationToken);
        }

        public UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken)
        {
            return _history.IsPremium ? UniTask.FromResult(false) : _inner.ShowInterstitialAsync(cancellationToken);
        }

        public void Dispose()
        {
            _inner.AvailabilityChanged -= OnAvailabilityChanged;
            _history.Changed -= OnAvailabilityChanged;
        }

        private void OnAvailabilityChanged()
        {
            AvailabilityChanged?.Invoke();
        }
    }
}
