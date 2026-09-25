using System;

namespace Hauntscope.Core.Observables
{
    public interface IReadOnlyObservableValue<out T>
    {
        event Action<T> Changed;

        T Value { get; }
    }
}
