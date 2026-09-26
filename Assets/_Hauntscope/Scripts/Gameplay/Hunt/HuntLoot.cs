using System.Collections.Generic;
using Hauntscope.Core.Observables;

namespace Hauntscope.Gameplay.Hunt
{
    // What was picked up around the room during the current hunt: the ectoplasm, paid out on the result card whatever
    // the outcome, and which pickups they were, for the contracts.
    public sealed class HuntLoot
    {
        private readonly ObservableValue<int> _ectoplasm = new ObservableValue<int>();
        private readonly List<string> _pickups = new List<string>();

        public IReadOnlyObservableValue<int> Ectoplasm => _ectoplasm;

        public IReadOnlyList<string> Pickups => _pickups;

        public void RecordPickup(string id)
        {
            _pickups.Add(id);
        }

        public void Add(int amount)
        {
            if (amount > 0)
                _ectoplasm.Value += amount;
        }

        public void Reset()
        {
            _ectoplasm.Value = 0;
            _pickups.Clear();
        }
    }
}
