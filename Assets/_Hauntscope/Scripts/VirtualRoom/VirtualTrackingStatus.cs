using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.VirtualRoom
{
    public sealed class VirtualTrackingStatus : ITrackingStatus
    {
        private readonly ObservableValue<bool> _isTracking = new ObservableValue<bool>(true);

        public IReadOnlyObservableValue<bool> IsTracking => _isTracking;
    }
}
