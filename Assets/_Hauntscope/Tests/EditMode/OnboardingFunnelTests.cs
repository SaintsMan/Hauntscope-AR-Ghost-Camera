using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Onboarding;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class OnboardingFunnelTests
    {
        private FakeAnalyticsService _analytics;
        private PlayerProgress _progress;
        private OnboardingFunnel _funnel;

        [SetUp]
        public void SetUp()
        {
            _analytics = new FakeAnalyticsService();
            _progress = new PlayerProgress();
            _funnel = new OnboardingFunnel(_analytics, _progress);
        }

        [Test]
        public void Step_NewAgent_LogsTheStepName()
        {
            _funnel.Step(OnboardingFunnel.SplashDone);

            Assert.AreEqual(1, _analytics.Count(AnalyticsNames.OnboardingStep));
            Assert.AreEqual("splash_done", _analytics.ParameterOf(AnalyticsNames.OnboardingStep, AnalyticsNames.Step));
        }

        [Test]
        public void Step_WithAChoice_LogsBoth()
        {
            _funnel.Step(OnboardingFunnel.LaunchChoice, OnboardingFunnel.ChoiceVirtual);

            Assert.AreEqual("launch_choice", _analytics.ParameterOf(AnalyticsNames.OnboardingStep, AnalyticsNames.Step));
            Assert.AreEqual("virtual", _analytics.ParameterOf(AnalyticsNames.OnboardingStep, AnalyticsNames.Choice));
        }

        [Test]
        public void Step_TutorialAlreadyDone_LogsNothing()
        {
            _progress.MarkTutorialCompleted();

            _funnel.Step(OnboardingFunnel.HuntTap);
            _funnel.Step(OnboardingFunnel.LaunchChoice, OnboardingFunnel.ChoiceAr);

            Assert.AreEqual(0, _analytics.Count(AnalyticsNames.OnboardingStep));
        }

        [Test]
        public void IsOnboarding_FirstHuntPlayedButTutorialNotFinished_StaysTrue()
        {
            _progress.RegisterSession();

            Assert.IsTrue(_funnel.IsOnboarding);
        }
    }
}
