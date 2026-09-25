using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntResult
    {
        public HuntResult(HuntOutcome outcome, GhostData ghost, float duration)
        {
            Outcome = outcome;
            Ghost = ghost;
            Duration = duration;
        }

        public HuntOutcome Outcome { get; }

        public GhostData Ghost { get; }

        public float Duration { get; }

        public int Reward => Outcome == HuntOutcome.Captured ? Ghost.Capture.Reward : 0;
    }
}
