using System;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Hunt
{
    // GDD 5.11: the hunt stays paused while at least one reason is active.
    public sealed class HuntPause : IStartable, ITickable, IDisposable
    {
        private const PauseReason MenuReasons = PauseReason.Manual | PauseReason.Background;

        private readonly ITrackingStatus _tracking;
        private readonly IApplicationLifecycle _lifecycle;
        private readonly TrackingConfig _config;
        private readonly ObservableValue<PauseReason> _reasons = new ObservableValue<PauseReason>(PauseReason.None);

        private bool _hasTracked;
        private float _untrackedTime;

        public HuntPause(ITrackingStatus tracking, IApplicationLifecycle lifecycle, TrackingConfig config)
        {
            _tracking = tracking;
            _lifecycle = lifecycle;
            _config = config;
        }

        public IReadOnlyObservableValue<PauseReason> Reasons => _reasons;

        public bool IsPaused => _reasons.Value != PauseReason.None;

        public bool IsMenuRequested => Has(MenuReasons);

        public bool Has(PauseReason reason)
        {
            return (_reasons.Value & reason) != 0;
        }

        public void Start()
        {
            _hasTracked = _tracking.IsTracking.Value;
            _tracking.IsTracking.Changed += OnTrackingChanged;
            _lifecycle.Paused += OnApplicationPaused;
        }

        void ITickable.Tick()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            // The session starts untracked while ARCore initialises; only a loss after tracking was acquired counts.
            if (!_hasTracked || _tracking.IsTracking.Value || Has(PauseReason.TrackingLost))
                return;

            _untrackedTime += deltaTime;
            if (_untrackedTime >= _config.LostGrace)
                Add(PauseReason.TrackingLost);
        }

        public void PauseManually()
        {
            Add(PauseReason.Manual);
        }

        public void Resume()
        {
            Remove(MenuReasons);
        }

        // Android Back: opens the pause menu during the hunt and closes it again, like the pause and resume buttons.
        public void TogglePause()
        {
            if (IsMenuRequested)
                Resume();
            else
                PauseManually();
        }

        public void Dispose()
        {
            _tracking.IsTracking.Changed -= OnTrackingChanged;
            _lifecycle.Paused -= OnApplicationPaused;
        }

        private void OnTrackingChanged(bool tracking)
        {
            _untrackedTime = 0f;
            if (!tracking)
                return;

            _hasTracked = true;
            Remove(PauseReason.TrackingLost);
        }

        private void OnApplicationPaused()
        {
            Add(PauseReason.Background);
        }

        private void Add(PauseReason reason)
        {
            _reasons.Value |= reason;
        }

        private void Remove(PauseReason reason)
        {
            _reasons.Value &= ~reason;
        }
    }
}
