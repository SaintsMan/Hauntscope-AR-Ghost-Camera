using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // The first hunts, phase by phase (hunt_phase): the room found, the ghost there, the radar on it, the lens showing
    // it, the beam holding it, the last fight. Each step once per ghost, with the seconds since the hunt scene opened,
    // so a stuck new agent shows where and when: the tutorial steps alone could not tell a lost ghost from a lost aim.
    public sealed class HuntPhaseFunnel : IStartable, IDisposable
    {
        public const string RoomReady = "room_ready";
        public const string GhostSpawn = "ghost_spawn";
        public const string EmfFound = "emf_found";
        public const string GhostSeen = "ghost_seen";
        public const string BeamLock = "beam_lock";
        public const string Surge = "surge";

        private readonly HuntSession _session;
        private readonly RoomCalibration _calibration;
        private readonly EmfRadar _radar;
        private readonly CaptureBeam _beam;
        private readonly PlayerProgress _progress;
        private readonly OnboardingConfig _config;
        private readonly HuntLaunchOptions _options;
        private readonly IAnalyticsService _analytics;
        private readonly IClock _clock;
        private DateTime _startedAt;
        private bool _isReporting;
        private bool _roomReported;
        private bool _emfReported;
        private bool _lockReported;
        private bool _surgeReported;
        private Ghost _ghost;

        public HuntPhaseFunnel(HuntSession session, RoomCalibration calibration, EmfRadar radar, CaptureBeam beam, PlayerProgress progress,
            OnboardingConfig config, HuntLaunchOptions options, IAnalyticsService analytics, IClock clock)
        {
            _session = session;
            _calibration = calibration;
            _radar = radar;
            _beam = beam;
            _progress = progress;
            _config = config;
            _options = options;
            _analytics = analytics;
            _clock = clock;
        }

        public void Start()
        {
            // The hunt counter moves when the ghost comes, so the scan of the first hunt still reads 0 here.
            _isReporting = _progress.TotalSessions < _config.PhaseFunnelHunts;
            if (!_isReporting)
                return;

            _startedAt = _clock.UtcNow;
            _calibration.Progress.Changed += OnCalibrationChanged;
            _session.Ghost.Changed += OnGhostChanged;
            _session.Sighted += OnSighted;
            _radar.Level.Changed += OnEmfLevelChanged;
            _beam.IsLocked.Changed += OnLockChanged;
            OnCalibrationChanged(_calibration.Progress.Value);
        }

        public void Dispose()
        {
            if (!_isReporting)
                return;

            _calibration.Progress.Changed -= OnCalibrationChanged;
            _session.Ghost.Changed -= OnGhostChanged;
            _session.Sighted -= OnSighted;
            _radar.Level.Changed -= OnEmfLevelChanged;
            _beam.IsLocked.Changed -= OnLockChanged;
            Watch(null);
        }

        private void OnCalibrationChanged(float progress)
        {
            if (_roomReported || progress < 1f)
                return;

            _roomReported = true;
            Report(RoomReady);
        }

        private void OnGhostChanged(Ghost ghost)
        {
            Watch(ghost);
            if (ghost == null)
                return;

            _emfReported = false;
            _lockReported = false;
            _surgeReported = false;
            Report(GhostSpawn);
        }

        private void OnSighted()
        {
            Report(GhostSeen);
        }

        private void OnEmfLevelChanged(int level)
        {
            if (_emfReported || _ghost == null || level < _config.PhaseFunnelEmfLevel)
                return;

            _emfReported = true;
            Report(EmfFound);
        }

        private void OnLockChanged(bool locked)
        {
            if (_lockReported || !locked || _ghost == null)
                return;

            _lockReported = true;
            Report(BeamLock);
        }

        private void OnSurgeStarted()
        {
            if (_surgeReported)
                return;

            _surgeReported = true;
            Report(Surge);
        }

        private void Watch(Ghost ghost)
        {
            if (_ghost != null)
                _ghost.SurgeStarted -= OnSurgeStarted;
            _ghost = ghost;
            if (_ghost != null)
                _ghost.SurgeStarted += OnSurgeStarted;
        }

        private void Report(string step)
        {
            var seconds = (long)Math.Round((_clock.UtcNow - _startedAt).TotalSeconds);
            _analytics.Log(AnalyticsNames.HuntPhase,
                AnalyticsParameter.Of(AnalyticsNames.Step, step),
                AnalyticsParameter.Of(AnalyticsNames.Seconds, seconds),
                AnalyticsParameter.Of(AnalyticsNames.HuntNumber, _progress.TotalSessions),
                AnalyticsParameter.Of(AnalyticsNames.Environment, AnalyticsNames.Of(_options.Environment)));
        }
    }
}
