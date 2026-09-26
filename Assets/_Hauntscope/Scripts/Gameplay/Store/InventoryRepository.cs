using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Store
{
    public sealed class InventoryRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "player_inventory";

        private readonly ISaveService _save;
        private readonly StoreConfig _store;

        public InventoryRepository(ISaveService save, StoreConfig store)
        {
            _save = save;
            _store = store;
        }

        // The standard laser is always owned, and a save that equips something unknown (removed from the store)
        // falls back to it, so the hunt never starts without a beam.
        public PlayerInventory Load()
        {
            var defaultLaser = _store.DefaultLaser != null ? _store.DefaultLaser.Id : string.Empty;
            if (!_save.TryLoad<PlayerInventoryDto>(Key, out var dto) || !IsSupported(dto.Version))
                return new PlayerInventory(defaultLaser);

            var lasers = new List<string> { defaultLaser };
            foreach (var id in dto.Lasers)
            {
                if (!string.IsNullOrEmpty(id) && _store.FindLaser(id) != null)
                    lasers.Add(id);
            }

            var gear = new Dictionary<string, int>();
            foreach (var entry in dto.Gear)
            {
                if (!string.IsNullOrEmpty(entry.GearId) && entry.Count > 0)
                    gear[entry.GearId] = entry.Count;
            }

            var equipped = lasers.Contains(dto.EquippedLaserId) ? dto.EquippedLaserId : defaultLaser;
            return new PlayerInventory(lasers, equipped, gear, dto.Armed);
        }

        public void Save(PlayerInventory inventory)
        {
            var gear = new List<GearCountDto>(inventory.Gear.Count);
            foreach (var pair in inventory.Gear)
                gear.Add(new GearCountDto(pair.Key, pair.Value));

            _save.Save(Key, new PlayerInventoryDto(CurrentVersion, new List<string>(inventory.Lasers),
                inventory.EquippedLaserId.Value, gear, new List<string>(inventory.Armed)));
        }

        private static bool IsSupported(int version)
        {
            return version >= 1 && version <= CurrentVersion;
        }
    }
}
