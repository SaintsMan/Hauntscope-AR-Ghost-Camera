using System;
using Hauntscope.Core.Services;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // ads_init per attempt to start ads (result ok / no_consent / error, attempt n): the share of sessions that never
    // get ads was only visible as a non-fatal in Crashlytics. rewarded_watch per rewarded ad a placement asked for
    // (placement, result), since GA4's own ad_reward has no context.
    public sealed class AdsAnalytics : IStartable, IDisposable
    {
        private readonly IAdsInitReport _report;
        private readonly IRewardedReport _rewarded;
        private readonly IAnalyticsService _analytics;

        public AdsAnalytics(IAdsInitReport report, IRewardedReport rewarded, IAnalyticsService analytics)
        {
            _report = report;
            _rewarded = rewarded;
            _analytics = analytics;
        }

        public void Start()
        {
            _report.Reported += OnReported;
            _rewarded.Watched += OnWatched;
        }

        public void Dispose()
        {
            _report.Reported -= OnReported;
            _rewarded.Watched -= OnWatched;
        }

        private void OnWatched(string placement, string result)
        {
            _analytics.Log(AnalyticsNames.RewardedWatch,
                AnalyticsParameter.Of(AnalyticsNames.Placement, placement),
                AnalyticsParameter.Of(AnalyticsNames.Result, result));
        }

        private void OnReported(string result, int attempt)
        {
            _analytics.Log(AnalyticsNames.AdsInit,
                AnalyticsParameter.Of(AnalyticsNames.Result, result),
                AnalyticsParameter.Of(AnalyticsNames.Attempt, attempt));
        }
    }
}
