using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Iap;

namespace Hauntscope.Gameplay.Ads
{
    // Leaving the result card counts one finished hunt and plays an interstitial when the policy allows it. The screen
    // switches off first and swallows taps for a moment, then the ad opens: the tap that left the card can never become
    // an ad click.
    public sealed class AdBreak
    {
        private readonly IAdsService _ads;
        private readonly AdPacing _pacing;
        private readonly InterstitialPolicy _policy;
        private readonly OfferMoments _moments;
        private readonly IScreenTransition _transition;
        private readonly AdsConfig _config;

        public AdBreak(IAdsService ads, AdPacing pacing, InterstitialPolicy policy, OfferMoments moments, IScreenTransition transition,
            AdsConfig config)
        {
            _moments = moments;
            _ads = ads;
            _pacing = pacing;
            _policy = policy;
            _transition = transition;
            _config = config;
        }

        // keepCovered: the caller loads another scene next, which switches the screen back on by itself.
        public async UniTask TryShowAsync(bool keepCovered, CancellationToken cancellationToken)
        {
            _pacing.RegisterHunt();
            if (!_policy.ShouldShow() || !_ads.IsInterstitialReady)
                return;

            var leadIn = _config.InterstitialLeadIn > 0f
                ? UniTask.Delay(TimeSpan.FromSeconds(_config.InterstitialLeadIn), DelayType.Realtime, cancellationToken: cancellationToken)
                : UniTask.CompletedTask;
            await UniTask.WhenAll(_transition.CoverAsync(cancellationToken), leadIn);
            if (await _ads.ShowInterstitialAsync(cancellationToken))
                _moments.RecordAdBreak();
            if (!keepCovered)
                await _transition.RevealAsync(cancellationToken);
        }
    }
}
