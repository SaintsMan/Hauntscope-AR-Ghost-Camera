using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeTrackingStatus : ITrackingStatus
    {
        private readonly ObservableValue<bool> _isTracking = new ObservableValue<bool>();

        public IReadOnlyObservableValue<bool> IsTracking => _isTracking;

        public void SetTracking(bool tracking)
        {
            _isTracking.Value = tracking;
        }
    }
}
