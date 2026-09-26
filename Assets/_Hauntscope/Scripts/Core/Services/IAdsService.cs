using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    public interface IAdsService
    {
        bool IsRewardedReady { get; }

        bool IsInterstitialReady { get; }

        // A full-screen ad is on screen: the game is paused by the OS, but must not open its own pause menu.
        bool IsShowing { get; }

        event Action AvailabilityChanged;

        // Consent, then the SDK. True when ads may be requested for this player.
        UniTask<bool> InitializeAsync(CancellationToken cancellationToken);

        // True when the player watched long enough to earn the reward.
        UniTask<bool> ShowRewardedAsync(CancellationToken cancellationToken);

        // True when an ad was actually shown.
        UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken);
    }
}
