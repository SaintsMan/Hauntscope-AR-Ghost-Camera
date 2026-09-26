using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using UnityEngine;

namespace Hauntscope.Infrastructure.Ads
{
    // Editor and non-Android builds: an "ad" is a short wait that always pays out, so every placement can be
    // played through without the SDK. AdsConfig.EditorAdsAvailable switches ads off to try the no-fill path.
    public sealed class EditorAdsService : IAdsService, IAdPrivacy
    {
        private const int MillisecondsPerSecond = 1000;

        private readonly AdsConfig _config;

        public EditorAdsService(AdsConfig config)
        {
            _config = config;
        }

        public bool IsRewardedReady => _config.EditorAdsAvailable && !IsShowing;

        public bool IsInterstitialReady => _config.EditorAdsAvailable && !IsShowing;

        public bool IsShowing { get; private set; }

        public bool IsOptionsRequired => false;

        public event Action AvailabilityChanged;

        public UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(_config.EditorAdsAvailable);
        }

        public async UniTask<bool> ShowRewardedAsync(CancellationToken cancellationToken)
        {
            return await PlayAsync("rewarded", cancellationToken);
        }

        public async UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken)
        {
            return await PlayAsync("interstitial", cancellationToken);
        }

        public UniTask ShowOptionsAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        private async UniTask<bool> PlayAsync(string format, CancellationToken cancellationToken)
        {
            if (!_config.EditorAdsAvailable || IsShowing)
                return false;

            Debug.Log($"Hauntscope: [editor ad] {format}");
            IsShowing = true;
            AvailabilityChanged?.Invoke();
            try
            {
                await UniTask.Delay((int)(_config.EditorAdDuration * MillisecondsPerSecond), true, cancellationToken: cancellationToken);
                return true;
            }
            finally
            {
                IsShowing = false;
                AvailabilityChanged?.Invoke();
            }
        }
    }
}
