using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Photo;
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
            bool isDoubled = false,
            bool isDeclassified = false,
            PhotoShot bestPhoto = null,
            int photoReward = 0,
            int photoEvidence = 0,
            float nightMultiplier = 1f)
        {
            NightMultiplier = nightMultiplier;
            BestPhoto = bestPhoto;
            PhotoReward = photoReward;
            PhotoEvidence = photoEvidence;
            IsDeclassified = isDeclassified;
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

        // Witching hour bonus on the catch (1 = none), shown on its own line of the breakdown.
        public float NightMultiplier { get; }

        // This catch declassified the ghost's Bestiary file.
        public bool IsDeclassified { get; }

        // A rewarded ad doubled the capture reward (not the pickups).
        public bool IsDoubled { get; }

        // The best shot of the hunt, kept even when the ghost got away.
        public PhotoShot BestPhoto { get; }

        public int PhotoReward { get; }

        // Good shots that count towards declassifying the ghost's file.
        public int PhotoEvidence { get; }

        public int BaseReward => Outcome == HuntOutcome.Captured ? Ghost.Capture.Reward : 0;

        public int CaptureReward => Mathf.RoundToInt(BaseReward * RewardMultiplier * NightMultiplier) * (IsDoubled ? 2 : 1);

        // The parts of the capture reward above the base, as the result card lists them (doubling included).
        public int ResearchBonus => (Mathf.RoundToInt(BaseReward * RewardMultiplier) - BaseReward) * (IsDoubled ? 2 : 1);

        public int NightBonus => CaptureReward - BaseReward * (IsDoubled ? 2 : 1) - ResearchBonus;

        public int Reward => CaptureReward + Found + PhotoReward;

        public HuntResult WithDoubledCapture()
        {
            return new HuntResult(Outcome, Ghost, Duration, IsFirstCapture, Found, RewardMultiplier, true, IsDeclassified,
                BestPhoto, PhotoReward, PhotoEvidence, NightMultiplier);
        }
    }
}
