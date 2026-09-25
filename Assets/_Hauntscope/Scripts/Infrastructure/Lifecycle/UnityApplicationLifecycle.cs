using System;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Infrastructure.Lifecycle
{
    public sealed class UnityApplicationLifecycle : MonoBehaviour, IApplicationLifecycle
    {
        private bool _isPaused;

        public event Action Paused;

        public event Action Resumed;

        private void OnApplicationPause(bool pauseStatus)
        {
            // Android also sends OnApplicationPause(false) on startup; only a real pause/resume pair is reported.
            if (pauseStatus == _isPaused)
                return;

            _isPaused = pauseStatus;
            if (pauseStatus)
                Paused?.Invoke();
            else
                Resumed?.Invoke();
        }
    }
}
