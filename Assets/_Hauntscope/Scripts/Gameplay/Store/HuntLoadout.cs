using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Store
{
    // What the agent carries into the current hunt: the equipped laser always, plus the armed boosters, which are
    // used up when a ghost appears (not at the scan, so a scan abandoned halfway costs nothing).
    public sealed class HuntLoadout
    {
        private readonly PlayerInventory _inventory;
        private readonly InventoryRepository _repository;
        private readonly StoreConfig _store;
        private readonly HuntModifiers _modifiers;
        private readonly List<BoosterData> _activeBoosters = new List<BoosterData>();

        public HuntLoadout(PlayerInventory inventory, InventoryRepository repository, StoreConfig store, HuntModifiers modifiers)
        {
            _inventory = inventory;
            _repository = repository;
            _store = store;
            _modifiers = modifiers;
            Laser = _store.FindLaser(_inventory.EquippedLaserId.Value) ?? _store.DefaultLaser;
            ApplyLaser();
        }

        public event Action Changed;

        public LaserData Laser { get; }

        public IReadOnlyList<BoosterData> ActiveBoosters => _activeBoosters;

        // A night shift uses its boosters up at its start only and keeps them for the rounds after (GDD 5.27).
        public void Begin(bool consumeBoosters = true)
        {
            ApplyLaser();
            if (!consumeBoosters)
            {
                foreach (var booster in _activeBoosters)
                    _modifiers.Apply(booster.Modifiers);
                Changed?.Invoke();
                return;
            }

            _activeBoosters.Clear();
            foreach (var booster in _store.Boosters)
            {
                if (!_inventory.IsArmed(booster.Id) || !_inventory.TryConsume(booster.Id))
                    continue;

                _activeBoosters.Add(booster);
                _modifiers.Apply(booster.Modifiers);
            }

            if (_activeBoosters.Count > 0)
                _repository.Save(_inventory);
            Changed?.Invoke();
        }

        private void ApplyLaser()
        {
            _modifiers.Reset();
            if (Laser != null)
                _modifiers.Apply(Laser.Modifiers);
        }
    }
}
