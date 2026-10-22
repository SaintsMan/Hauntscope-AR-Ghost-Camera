using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Onboarding;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // FeatureUnlocks on the game's default thresholds over a player's progress, with the pieces a test may want to look at.
    public sealed class FeatureFactory
    {
        public FeatureFactory(PlayerProgress progress = null)
        {
            Progress = progress ?? new PlayerProgress();
            Save = new FakeSaveService();
            Repository = new PlayerProgressRepository(Save);
            Remote = new FakeRemoteValues();
            Analytics = new FakeAnalyticsService();
            Config = new OnboardingConfig();
            Features = new FeatureUnlocks(Progress, Repository, Config, new ShiftConfig(), new PrankPhotoConfig(), new RemoteTuning(Remote),
                Analytics);
        }

        public PlayerProgress Progress { get; }

        public FakeSaveService Save { get; }

        public PlayerProgressRepository Repository { get; }

        public FakeRemoteValues Remote { get; }

        public FakeAnalyticsService Analytics { get; }

        public OnboardingConfig Config { get; }

        public FeatureUnlocks Features { get; }

        public FirstHunt FirstHunt => new FirstHunt(Progress, Config, new RemoteTuning(Remote));

        public void PlayHunts(int count)
        {
            for (var i = 0; i < count; i++)
                Progress.RegisterSession();
        }
    }
}
