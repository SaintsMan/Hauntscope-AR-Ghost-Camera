using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // first_capture: the moment a new agent catches the first ghost, with seconds since install and the hunt it took,
    // so LiveOps can see how long the way to the first catch is, build by build. Agents who caught before this existed
    // never send it.
    public sealed class FirstCaptureAnalytics : IStartable, IDisposable
    {
        private readonly PlayerProgress _progress;
        private readonly IInstallInfo _install;
        private readonly IClock _clock;
        private readonly IAnalyticsService _analytics;
        private bool _hadCapture;

        public FirstCaptureAnalytics(PlayerProgress progress, IInstallInfo install, IClock clock, IAnalyticsService analytics)
        {
            _progress = progress;
            _install = install;
            _clock = clock;
            _analytics = analytics;
        }

        public void Start()
        {
            _hadCapture = _progress.TotalCaptures > 0;
            _progress.Changed += OnProgressChanged;
        }

        public void Dispose()
        {
            _progress.Changed -= OnProgressChanged;
        }

        private void OnProgressChanged()
        {
            if (_hadCapture || _progress.TotalCaptures == 0)
                return;

            _hadCapture = true;
            var huntNumber = AnalyticsParameter.Of(AnalyticsNames.HuntNumber, _progress.TotalSessions);
            if (_install.TryGetInstalledAt(out var installed))
            {
                var seconds = (long)Math.Max(0d, (_clock.UtcNow - installed).TotalSeconds);
                _analytics.Log(AnalyticsNames.FirstCapture, huntNumber, AnalyticsParameter.Of(AnalyticsNames.SinceInstallSeconds, seconds));
            }
            else
            {
                _analytics.Log(AnalyticsNames.FirstCapture, huntNumber);
            }
        }
    }
}
