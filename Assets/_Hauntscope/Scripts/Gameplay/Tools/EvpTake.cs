using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Tools
{
    // What a take caught: the ghost's voice when it came close enough, otherwise only tape noise.
    public readonly struct EvpTake
    {
        public EvpTake(bool hasVoice, GhostData ghost)
        {
            HasVoice = hasVoice;
            Ghost = hasVoice ? ghost : null;
        }

        public bool HasVoice { get; }

        // Whose voice it is; null on a tape of static.
        public GhostData Ghost { get; }
    }
}
