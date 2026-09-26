using Hauntscope.Gameplay.Config;
using UnityEngine;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntResult
    {
        public HuntResult(
            HuntOutcome outcome,
            GhostData ghost,
            float duration,
            bool isFirstCapture = false,
            int found = 0,
            float rewardMultiplier = 1f,
            bool isDoubled = false)
        {
            IsFirstCapture = isFirstCapture;
            Outcome = outcome;
            Ghost = ghost;
            Duration = duration;
            Found = found;
            RewardMultiplier = rewardMultiplier;
            IsDoubled = isDoubled;
        }

        public HuntOutcome Outcome { get; }

        public GhostData Ghost { get; }

        public float Duration { get; }

        // A ghost caught for the first time opens a new Bestiary entry.
        public bool IsFirstCapture { get; }

        // Ectoplasm picked up around the room; kept even when the ghost got away.
        public int Found { get; }

        // Research bonus for declassified ghosts (1 = none).
        public float RewardMultiplier { get; }

        // A rewarded ad doubled the capture reward (not the pickups).
        public bool IsDoubled { get; }

        public int BaseReward => Outcome == HuntOutcome.Captured ? Ghost.Capture.Reward : 0;

        public int CaptureReward => Mathf.RoundToInt(BaseReward * RewardMultiplier) * (IsDoubled ? 2 : 1);

        public int Reward => CaptureReward + Found;

        public HuntResult WithDoubledCapture()
        {
            return new HuntResult(Outcome, Ghost, Duration, IsFirstCapture, Found, RewardMultiplier, true);
        }
    }
}
