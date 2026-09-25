using System;
using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Environment;
using UnityEngine.XR.ARFoundation;

namespace Hauntscope.AR
{
    public sealed class ArTrackingStatus : ITrackingStatus, IDisposable
    {
        private readonly ObservableValue<bool> _isTracking;

        public ArTrackingStatus()
        {
            _isTracking = new ObservableValue<bool>(IsSessionTracking(ARSession.state));
            ARSession.stateChanged += OnSessionStateChanged;
        }

        public IReadOnlyObservableValue<bool> IsTracking => _isTracking;

        public void Dispose()
        {
            ARSession.stateChanged -= OnSessionStateChanged;
        }

        private void OnSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            _isTracking.Value = IsSessionTracking(args.state);
        }

        private static bool IsSessionTracking(ARSessionState state)
        {
            return state == ARSessionState.SessionTracking;
        }
    }
}
