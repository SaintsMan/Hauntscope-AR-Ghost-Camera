using Hauntscope.Core.Observables;

namespace Hauntscope.Gameplay.Tools
{
    public interface ITool
    {
        IReadOnlyObservableValue<bool> IsActive { get; }

        float DrainPerSecond { get; }

        void Activate();

        void Deactivate();

        void Tick(float deltaTime);
    }
}
