using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Ads
{
    // Decorator: every ad that actually plays is recorded for interstitial pacing, whoever showed it, without the
    // AdMob adapter or the placements knowing about pacing.
    public sealed class PacedAdsService : IAdsService
    {
        private readonly IAdsService _inner;
        private readonly AdPacing _pacing;
        private readonly IClock _clock;

        public PacedAdsService(IAdsService inner, AdPacing pacing, IClock clock)
        {
            _inner = inner;
            _pacing = pacing;
            _clock = clock;
        }

        public bool IsRewardedReady => _inner.IsRewardedReady;

        public bool IsInterstitialReady => _inner.IsInterstitialReady;

        public bool IsShowing => _inner.IsShowing;

        public event Action AvailabilityChanged
        {
            add => _inner.AvailabilityChanged += value;
            remove => _inner.AvailabilityChanged -= value;
        }

        public UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            return _inner.InitializeAsync(cancellationToken);
        }

        // A rewarded ad closed early still played, so it counts for pacing even without a reward.
        public async UniTask<bool> ShowRewardedAsync(CancellationToken cancellationToken)
        {
            var ready = _inner.IsRewardedReady;
            var rewarded = await _inner.ShowRewardedAsync(cancellationToken);
            if (ready)
                _pacing.MarkShown(_clock.UtcNow, false);
            return rewarded;
        }

        public async UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken)
        {
            var shown = await _inner.ShowInterstitialAsync(cancellationToken);
            if (shown)
                _pacing.MarkShown(_clock.UtcNow, true);
            return shown;
        }
    }
}
