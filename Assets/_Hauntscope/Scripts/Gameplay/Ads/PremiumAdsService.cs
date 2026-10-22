using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Iap;

namespace Hauntscope.Gameplay.Ads
{
    // Decorator for the full version: no ad breaks, and every "watch a video" reward is granted without the video.
    // Placements keep asking the same IAdsService and never learn the player paid.
    public sealed class PremiumAdsService : IAdsService, IRewardedReport, IDisposable
    {
        public const string EarnedResult = "earned";
        public const string SkippedResult = "skipped";
        public const string NotReadyResult = "not_ready";
        public const string PremiumResult = "premium";

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

        public event Action<string, string> Watched;

        public bool IsRewardedReady => _history.IsPremium || _inner.IsRewardedReady;

        public bool IsInterstitialReady => !_history.IsPremium && _inner.IsInterstitialReady;

        public bool IsShowing => _inner.IsShowing;

        public UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            return _inner.InitializeAsync(cancellationToken);
        }

        // The outermost layer, so the report sees every reward a placement asked for, premium ones included.
        public async UniTask<bool> ShowRewardedAsync(CancellationToken cancellationToken, string placement = "")
        {
            if (_history.IsPremium)
            {
                Watched?.Invoke(placement, PremiumResult);
                return true;
            }

            var ready = _inner.IsRewardedReady;
            var rewarded = await _inner.ShowRewardedAsync(cancellationToken, placement);
            Watched?.Invoke(placement, rewarded ? EarnedResult : ready ? SkippedResult : NotReadyResult);
            return rewarded;
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
