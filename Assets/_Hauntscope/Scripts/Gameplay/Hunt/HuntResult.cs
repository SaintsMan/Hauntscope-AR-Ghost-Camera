using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntResult
    {
        public HuntResult(HuntOutcome outcome, GhostData ghost, float duration, bool isFirstCapture = false)
        {
            IsFirstCapture = isFirstCapture;
            Outcome = outcome;
            Ghost = ghost;
            Duration = duration;
        }

        public HuntOutcome Outcome { get; }

        public GhostData Ghost { get; }

        public float Duration { get; }

        // A ghost caught for the first time opens a new Bestiary entry.
        public bool IsFirstCapture { get; }

        public int Reward => Outcome == HuntOutcome.Captured ? Ghost.Capture.Reward : 0;
    }
}
