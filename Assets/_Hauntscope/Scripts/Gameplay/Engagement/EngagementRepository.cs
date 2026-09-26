using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Contracts;

namespace Hauntscope.Gameplay.Engagement
{
    public sealed class EngagementRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "engagement";

        private readonly ISaveService _save;
        private readonly ContractConfig _contracts;
        private readonly StoreConfig _store;

        public EngagementRepository(ISaveService save, ContractConfig contracts, StoreConfig store)
        {
            _save = save;
            _contracts = contracts;
            _store = store;
        }

        // A contract removed from the game since the save simply drops off the board; so does an unknown reward gear.
        public EngagementProgress Load()
        {
            if (!_save.TryLoad<EngagementDto>(Key, out var dto) || !IsSupported(dto.Version))
                return new EngagementProgress();

            var slots = new List<ContractSlot>();
            if (dto.Contracts != null)
            {
                foreach (var entry in dto.Contracts)
                {
                    var data = _contracts.Find(entry.ContractId);
                    if (data == null)
                        continue;

                    var reward = new RewardBundle(entry.Ectoplasm, _store.FindGear(entry.GearId), entry.GearCount);
                    slots.Add(new ContractSlot(data, entry.Subject, reward, entry.Progress, entry.Claimed));
                }
            }

            return new EngagementProgress(dto.ContractDay, slots, dto.FreeReplacesUsed, dto.AdReplacesUsed);
        }

        public void Save(EngagementProgress progress)
        {
            var slots = new List<ContractSlotDto>(progress.Contracts.Count);
            foreach (var slot in progress.Contracts)
            {
                var reward = slot.Reward;
                slots.Add(new ContractSlotDto(slot.Data.Id, slot.Subject, slot.Progress, slot.IsClaimed, reward.Ectoplasm,
                    reward.Gear != null ? reward.Gear.Id : string.Empty, reward.GearCount));
            }

            _save.Save(Key, new EngagementDto(CurrentVersion, progress.ContractDay, slots, progress.FreeReplacesUsed, progress.AdReplacesUsed));
        }

        // No migrations exist yet: version 1 is the only format. Unknown (newer or missing) versions start fresh.
        private static bool IsSupported(int version)
        {
            return version >= 1 && version <= CurrentVersion;
        }
    }
}
