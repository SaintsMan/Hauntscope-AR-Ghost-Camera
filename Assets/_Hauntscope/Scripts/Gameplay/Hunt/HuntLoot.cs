using Hauntscope.Core.Observables;

namespace Hauntscope.Gameplay.Hunt
{
    // Ectoplasm picked up around the room during the current hunt; paid out on the result card whatever the outcome.
    public sealed class HuntLoot
    {
        private readonly ObservableValue<int> _ectoplasm = new ObservableValue<int>();

        public IReadOnlyObservableValue<int> Ectoplasm => _ectoplasm;

        public void Add(int amount)
        {
            if (amount > 0)
                _ectoplasm.Value += amount;
        }

        public void Reset()
        {
            _ectoplasm.Value = 0;
        }
    }
}
