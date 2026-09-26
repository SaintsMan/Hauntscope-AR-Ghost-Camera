using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Ads
{
    // Leaving the result card counts one finished hunt and plays an interstitial when the policy allows it.
    public sealed class AdBreak
    {
        private readonly IAdsService _ads;
        private readonly AdPacing _pacing;
        private readonly InterstitialPolicy _policy;

        public AdBreak(IAdsService ads, AdPacing pacing, InterstitialPolicy policy)
        {
            _ads = ads;
            _pacing = pacing;
            _policy = policy;
        }

        public async UniTask TryShowAsync(CancellationToken cancellationToken)
        {
            _pacing.RegisterHunt();
            if (_policy.ShouldShow() && _ads.IsInterstitialReady)
                await _ads.ShowInterstitialAsync(cancellationToken);
        }
    }
}
