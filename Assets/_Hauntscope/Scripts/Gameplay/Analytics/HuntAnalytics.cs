using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // One hunt_start per visit to the Hunt scene and one hunt_end per ghost: caught, escaped, or left behind when the
    // player walked out of the scene mid-hunt.
    public sealed class HuntAnalytics : IStartable, IDisposable
    {
        private readonly HuntSession _session;
        private readonly HuntLaunchOptions _options;
        private readonly IAnalyticsService _analytics;
        private GhostData _unfinished;

        public HuntAnalytics(HuntSession session, HuntLaunchOptions options, IAnalyticsService analytics)
        {
            _session = session;
            _options = options;
            _analytics = analytics;
        }

        public void Start()
        {
            _session.Ghost.Changed += OnGhostChanged;
            _session.Result.Changed += OnResultChanged;
            _analytics.Log(AnalyticsNames.HuntStart,
                AnalyticsParameter.Of(AnalyticsNames.Environment, AnalyticsNames.Of(_options.Environment)),
                AnalyticsParameter.Of(AnalyticsNames.Mode, AnalyticsNames.Of(_options.Mode)));
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= OnGhostChanged;
            _session.Result.Changed -= OnResultChanged;
            if (_unfinished != null)
                LogEnd(AnalyticsNames.Quit, _unfinished, _session.Elapsed);
        }

        private void OnGhostChanged(Ghost ghost)
        {
            if (ghost != null && _session.Result.Value == null)
                _unfinished = _session.GhostData;
        }

        // The result is replaced once more when an ad doubles the reward; only the first one ends the hunt.
        private void OnResultChanged(HuntResult result)
        {
            if (result == null || _unfinished == null)
                return;

            _unfinished = null;
            LogEnd(AnalyticsNames.Of(result.Outcome), result.Ghost, result.Duration);
        }

        private void LogEnd(string outcome, GhostData ghost, float duration)
        {
            _analytics.Log(AnalyticsNames.HuntEnd,
                AnalyticsParameter.Of(AnalyticsNames.Outcome, outcome),
                AnalyticsParameter.Of(AnalyticsNames.Ghost, ghost != null ? ghost.Id : string.Empty),
                AnalyticsParameter.Of(AnalyticsNames.DurationSeconds, Mathf.RoundToInt(duration)));
        }
    }
}
