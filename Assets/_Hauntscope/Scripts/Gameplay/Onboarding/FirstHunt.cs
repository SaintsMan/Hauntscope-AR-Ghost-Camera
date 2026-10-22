using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Onboarding
{
    // The very first hunt of a new agent (GDD 5.41), and the two shortcuts the console may put in front of it: the
    // briefing's last page starts the hunt itself, and the hunt can begin in the Virtual Room, with no camera dialog.
    public sealed class FirstHunt
    {
        private readonly PlayerProgress _progress;
        private readonly OnboardingConfig _config;
        private readonly RemoteTuning _tuning;

        public FirstHunt(PlayerProgress progress, OnboardingConfig config, RemoteTuning tuning)
        {
            _progress = progress;
            _config = config;
            _tuning = tuning;
        }

        public bool IsPending => _progress.TotalSessions == 0 && !_progress.TutorialCompleted;

        public bool BriefingStartsHunt => IsPending && _tuning.Bool(RemoteKeys.BriefingStartsHunt, _config.BriefingStartsHunt);

        public bool StartsInVirtualRoom => IsPending && _tuning.Bool(RemoteKeys.FirstHuntVirtual, _config.FirstHuntVirtual);
    }
}
