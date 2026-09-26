using System;
using System.Collections.Generic;
using Hauntscope.Core.Observables;

namespace Hauntscope.Gameplay.Store
{
    // Everything the agent owns besides ectoplasm: lasers, the equipped one, gear stacks and which boosters to take along.
    public sealed class PlayerInventory
    {
        private readonly HashSet<string> _lasers;
        private readonly ObservableValue<string> _equippedLaserId;
        private readonly Dictionary<string, int> _gear;
        private readonly HashSet<string> _armed;

        public PlayerInventory(string defaultLaserId)
            : this(new[] { defaultLaserId }, defaultLaserId, new Dictionary<string, int>(), Array.Empty<string>())
        {
        }

        public PlayerInventory(
            IEnumerable<string> lasers,
            string equippedLaserId,
            IReadOnlyDictionary<string, int> gear,
            IEnumerable<string> armed)
        {
            _lasers = new HashSet<string>(lasers);
            _equippedLaserId = new ObservableValue<string>(equippedLaserId);
            _gear = new Dictionary<string, int>();
            foreach (var pair in gear)
            {
                if (pair.Value > 0)
                    _gear[pair.Key] = pair.Value;
            }

            _armed = new HashSet<string>(armed);
        }

        public event Action Changed;

        public IReadOnlyObservableValue<string> EquippedLaserId => _equippedLaserId;

        public IReadOnlyCollection<string> Lasers => _lasers;

        public IReadOnlyDictionary<string, int> Gear => _gear;

        public IReadOnlyCollection<string> Armed => _armed;

        public bool OwnsLaser(string laserId)
        {
            return _lasers.Contains(laserId);
        }

        public void AddLaser(string laserId)
        {
            if (_lasers.Add(laserId))
                Changed?.Invoke();
        }

        public bool EquipLaser(string laserId)
        {
            if (!OwnsLaser(laserId))
                return false;

            _equippedLaserId.Value = laserId;
            Changed?.Invoke();
            return true;
        }

        public int GetCount(string gearId)
        {
            return _gear.TryGetValue(gearId, out var count) ? count : 0;
        }

        public void AddGear(string gearId, int amount)
        {
            if (amount <= 0)
                return;

            _gear[gearId] = GetCount(gearId) + amount;
            Changed?.Invoke();
        }

        public bool TryConsume(string gearId)
        {
            var count = GetCount(gearId);
            if (count <= 0)
                return false;

            if (count == 1)
                _gear.Remove(gearId);
            else
                _gear[gearId] = count - 1;

            Changed?.Invoke();
            return true;
        }

        public bool IsArmed(string gearId)
        {
            return _armed.Contains(gearId);
        }

        public void SetArmed(string gearId, bool armed)
        {
            var changed = armed ? _armed.Add(gearId) : _armed.Remove(gearId);
            if (changed)
                Changed?.Invoke();
        }
    }
}
