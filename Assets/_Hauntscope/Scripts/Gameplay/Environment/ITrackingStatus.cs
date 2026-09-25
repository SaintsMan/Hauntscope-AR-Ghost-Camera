using Hauntscope.Core.Observables;

namespace Hauntscope.Gameplay.Environment
{
    public interface ITrackingStatus
    {
        IReadOnlyObservableValue<bool> IsTracking { get; }
    }
}
