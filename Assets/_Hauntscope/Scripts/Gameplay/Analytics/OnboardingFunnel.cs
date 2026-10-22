using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Analytics
{
    // Where new players leave (GDD 5.39, 5.41): one onboarding_step per milestone between the splash and the first ghost,
    // reported only until the tutorial hunt is done, so the funnel reads the same for every agent and never grows with
    // the veterans' hunts.
    public sealed class OnboardingFunnel
    {
        // Logged the moment the game shows anything, before consent, Firebase or the boot tasks: the queue sends it
        // once Firebase is up, so a player who leaves during the boot still counts.
        public const string SplashStart = "splash_start";
        public const string SplashDone = "splash_done";
        public const string BriefingDone = "briefing_done";
        public const string BriefingSkip = "briefing_skip";
        public const string BriefingStart = "briefing_start";
        public const string HuntTap = "hunt_tap";
        public const string CameraPrompt = "camera_prompt";
        public const string LaunchChoice = "launch_choice";
        public const string ArUnavailable = "ar_unavailable";
        public const string ScanStart = "scan_start";
        public const string GhostSpawn = "ghost_spawn";

        public const string ChoiceAr = "ar";
        public const string ChoiceVirtual = "virtual";
        public const string ChoiceBack = "back";

        private readonly IAnalyticsService _analytics;
        private readonly PlayerProgress _progress;

        public OnboardingFunnel(IAnalyticsService analytics, PlayerProgress progress)
        {
            _analytics = analytics;
            _progress = progress;
        }

        public bool IsOnboarding => !_progress.TutorialCompleted;

        public void Step(string step)
        {
            if (IsOnboarding)
                _analytics.Log(AnalyticsNames.OnboardingStep, AnalyticsParameter.Of(AnalyticsNames.Step, step));
        }

        public void Step(string step, string choice)
        {
            if (IsOnboarding)
            {
                _analytics.Log(AnalyticsNames.OnboardingStep, AnalyticsParameter.Of(AnalyticsNames.Step, step),
                    AnalyticsParameter.Of(AnalyticsNames.Choice, choice));
            }
        }
    }
}
