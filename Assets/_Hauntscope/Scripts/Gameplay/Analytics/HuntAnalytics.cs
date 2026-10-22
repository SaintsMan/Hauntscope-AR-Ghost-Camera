using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // One hunt_start per visit to the Hunt scene and one hunt_end per ghost: caught, escaped, or left behind when the
    // player walked out of the scene mid-hunt (quit_scan when no ghost had come yet). hunt_end also says how far the
    // beam got and how far the ghost was when the beam lost it, to tell a ghost that ran from one that was never aimed
    // at. A hunt the game never comes back to from the background is reported on the next launch (UnfinishedHuntLog).
    // Crash reports from here on say which camera and which kind of hunt.
    public sealed class HuntAnalytics : IStartable, IDisposable
    {
        private const int Percent = 100;
        private const float DecimetersPerMeter = 10f;
        private const int NoDistance = -1;

        private readonly HuntSession _session;
        private readonly HuntLaunchOptions _options;
        private readonly IAnalyticsService _analytics;
        private readonly ICrashReporter _reporter;
        private readonly CaptureBeam _beam;
        private readonly PlayerProgress _progress;
        private readonly IApplicationLifecycle _lifecycle;
        private readonly UnfinishedHuntLog _log;
        private GhostData _unfinished;
        private bool _hadGhost;
        private float _maxProgress;
        private int _distanceOnLostDm = NoDistance;

        public HuntAnalytics(HuntSession session, HuntLaunchOptions options, IAnalyticsService analytics, ICrashReporter reporter,
            CaptureBeam beam, PlayerProgress progress, IApplicationLifecycle lifecycle, UnfinishedHuntLog log)
        {
            _session = session;
            _options = options;
            _analytics = analytics;
            _reporter = reporter;
            _beam = beam;
            _progress = progress;
            _lifecycle = lifecycle;
            _log = log;
        }

        private string Environment => AnalyticsNames.Of(_options.Environment);

        public void Start()
        {
            _session.Ghost.Changed += OnGhostChanged;
            _session.Result.Changed += OnResultChanged;
            _beam.Progress.Changed += OnProgressChanged;
            _beam.IsLocked.Changed += OnLockChanged;
            _lifecycle.Paused += OnPaused;
            _lifecycle.Resumed += OnResumed;
            _reporter.SetKey(CrashKeys.Screen, CrashKeys.HuntScreen);
            _reporter.SetKey(CrashKeys.Mode, Environment);
            _reporter.SetKey(CrashKeys.HuntMode, AnalyticsNames.Of(_options.Mode));
            _analytics.SetUserProperty(AnalyticsNames.PlayModeProperty, Environment);
            _analytics.Log(AnalyticsNames.HuntStart,
                AnalyticsParameter.Of(AnalyticsNames.Environment, Environment),
                AnalyticsParameter.Of(AnalyticsNames.Mode, AnalyticsNames.Of(_options.Mode)));
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= OnGhostChanged;
            _session.Result.Changed -= OnResultChanged;
            _beam.Progress.Changed -= OnProgressChanged;
            _beam.IsLocked.Changed -= OnLockChanged;
            _lifecycle.Paused -= OnPaused;
            _lifecycle.Resumed -= OnResumed;
            _log.Forget();
            var record = Unfinished();
            if (record != null)
                LogEnd(_analytics, record, false);
        }

        public static void LogEnd(IAnalyticsService analytics, UnfinishedHuntDto record, bool fromBackground)
        {
            var parameters = new[]
            {
                AnalyticsParameter.Of(AnalyticsNames.Outcome, record.Outcome),
                AnalyticsParameter.Of(AnalyticsNames.Ghost, record.Ghost),
                AnalyticsParameter.Of(AnalyticsNames.DurationSeconds, record.DurationSeconds),
                AnalyticsParameter.Of(AnalyticsNames.Environment, record.Environment),
                AnalyticsParameter.Of(AnalyticsNames.HuntNumber, record.HuntNumber),
                AnalyticsParameter.Of(AnalyticsNames.MaxProgress, record.MaxProgress),
                AnalyticsParameter.Of(AnalyticsNames.DistanceOnLost, record.DistanceOnLostDm),
                AnalyticsParameter.Of(AnalyticsNames.Background, AnalyticsNames.Of(fromBackground))
            };
            analytics.Log(AnalyticsNames.HuntEnd, parameters);
        }

        private void OnGhostChanged(Ghost ghost)
        {
            if (ghost == null || _session.Result.Value != null)
                return;

            _unfinished = _session.GhostData;
            _hadGhost = true;
            _maxProgress = 0f;
            _distanceOnLostDm = NoDistance;
        }

        // The result is replaced once more when an ad doubles the reward; only the first one ends the hunt.
        private void OnResultChanged(HuntResult result)
        {
            if (result == null || _unfinished == null)
                return;

            _unfinished = null;
            _log.Forget();
            LogEnd(_analytics, Record(AnalyticsNames.Of(result.Outcome), result.Ghost, result.Duration), false);
        }

        private void OnProgressChanged(float progress)
        {
            _maxProgress = Mathf.Max(_maxProgress, progress);
        }

        private void OnLockChanged(bool locked)
        {
            if (!locked && _unfinished != null && _session.Result.Value == null)
                _distanceOnLostDm = Mathf.RoundToInt(_beam.GhostDistance * DecimetersPerMeter);
        }

        private void OnPaused()
        {
            var record = Unfinished();
            if (record != null)
                _log.Remember(record);
        }

        private void OnResumed()
        {
            _log.Forget();
        }

        // What hunt_end would say if the hunt stopped right now, or null when it already reported its end.
        private UnfinishedHuntDto Unfinished()
        {
            if (_unfinished != null)
                return Record(AnalyticsNames.Quit, _unfinished, _session.Elapsed);

            return _hadGhost ? null : Record(AnalyticsNames.QuitScan, null, 0f);
        }

        private UnfinishedHuntDto Record(string outcome, GhostData ghost, float duration)
        {
            return new UnfinishedHuntDto
            {
                Outcome = outcome,
                Ghost = ghost != null ? ghost.Id : string.Empty,
                DurationSeconds = Mathf.RoundToInt(duration),
                Environment = Environment,
                HuntNumber = _progress.TotalSessions,
                MaxProgress = Mathf.RoundToInt(_maxProgress * Percent),
                DistanceOnLostDm = _distanceOnLostDm
            };
        }
    }
}
