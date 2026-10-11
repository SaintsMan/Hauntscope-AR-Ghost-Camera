using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Hauntscope.AR
{
    // Follows the AR session through a hunt for the crash reports: every state change is a breadcrumb, and a session
    // that turns out unsupported or loses the camera is reported once, since AR on unknown phones is the main risk.
    public sealed class ArSessionDiagnostics : IDisposable
    {
        private readonly ICrashReporter _reporter;
        private bool _sessionReported;
        private bool _cameraReported;

        public ArSessionDiagnostics(ICrashReporter reporter)
        {
            _reporter = reporter;
            ARSession.stateChanged += OnStateChanged;
        }

        public void Dispose()
        {
            ARSession.stateChanged -= OnStateChanged;
        }

        private void OnStateChanged(ARSessionStateChangedEventArgs args)
        {
            var reason = ARSession.notTrackingReason;
            _reporter.Log($"AR session {args.state}, not tracking: {reason}");

            if (args.state == ARSessionState.Unsupported && !_sessionReported)
            {
                _sessionReported = true;
                _reporter.RecordNonFatal(CrashKeys.ArArea, "session unsupported during a hunt");
            }

            if (reason == NotTrackingReason.CameraUnavailable && !_cameraReported)
            {
                _cameraReported = true;
                _reporter.RecordNonFatal(CrashKeys.CameraArea, $"camera unavailable, session {args.state}");
            }
        }
    }
}
