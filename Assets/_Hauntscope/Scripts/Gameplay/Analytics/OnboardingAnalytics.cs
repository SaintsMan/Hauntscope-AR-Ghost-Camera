using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // The hunt half of the first-minutes funnel (GDD 5.41): the scan starting, the first ghost showing up, and each
    // hint of the tutorial hunt as the player reaches it. The menu half lives where the taps are (OnboardingFunnel).
    public sealed class OnboardingAnalytics : IStartable, IDisposable
    {
        private readonly HuntSession _session;
        private readonly TutorialFlow _tutorial;
        private readonly OnboardingFunnel _funnel;
        private readonly IAnalyticsService _analytics;
        private bool _ghostReported;

        public OnboardingAnalytics(HuntSession session, TutorialFlow tutorial, OnboardingFunnel funnel, IAnalyticsService analytics)
        {
            _session = session;
            _tutorial = tutorial;
            _funnel = funnel;
            _analytics = analytics;
        }

        public void Start()
        {
            _session.Ghost.Changed += OnGhostChanged;
            _tutorial.Step.Changed += OnTutorialStepChanged;
            _funnel.Step(OnboardingFunnel.ScanStart);
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= OnGhostChanged;
            _tutorial.Step.Changed -= OnTutorialStepChanged;
        }

        private void OnGhostChanged(Ghost ghost)
        {
            if (ghost == null || _ghostReported)
                return;

            _ghostReported = true;
            _funnel.Step(OnboardingFunnel.GhostSpawn);
        }

        private void OnTutorialStepChanged(TutorialStep step)
        {
            if (step == TutorialStep.None)
                return;

            _analytics.Log(AnalyticsNames.TutorialStep, AnalyticsParameter.Of(AnalyticsNames.Step, AnalyticsNames.Of(step)));
        }
    }
}
