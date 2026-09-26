using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // Every show completes at once with the configured outcome.
    public sealed class FakeAdsService : IAdsService
    {
        public bool IsRewardedReady { get; set; } = true;

        public bool IsInterstitialReady { get; set; } = true;

        public bool IsShowing { get; set; }

        public bool RewardEarned { get; set; } = true;

        public bool InitializeResult { get; set; } = true;

        public int RewardedShown { get; private set; }

        public int InterstitialsShown { get; private set; }

        public event Action AvailabilityChanged;

        public void RaiseAvailabilityChanged()
        {
            AvailabilityChanged?.Invoke();
        }

        public UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(InitializeResult);
        }

        public UniTask<bool> ShowRewardedAsync(CancellationToken cancellationToken)
        {
            if (!IsRewardedReady)
                return UniTask.FromResult(false);

            RewardedShown++;
            return UniTask.FromResult(RewardEarned);
        }

        public UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken)
        {
            if (!IsInterstitialReady)
                return UniTask.FromResult(false);

            InterstitialsShown++;
            return UniTask.FromResult(true);
        }
    }
}
