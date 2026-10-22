using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Season
{
    public sealed class HalloweenRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "halloween";

        private readonly ISaveService _save;

        public HalloweenRepository(ISaveService save)
        {
            _save = save;
        }

        // Unknown (newer or missing) versions start fresh: the event is short and its skins come back with a new purchase.
        public HalloweenProgress Load()
        {
            if (!_save.TryLoad<HalloweenDto>(Key, out var dto) || dto.Version < 1 || dto.Version > CurrentVersion)
                return new HalloweenProgress();

            return new HalloweenProgress(dto.Candy, dto.CandyEarned, dto.OwnedSkins, dto.Beam, dto.LensFrame, dto.Radar, dto.Burned, dto.Welcomed);
        }

        public void Save(HalloweenProgress progress)
        {
            _save.Save(Key, new HalloweenDto(CurrentVersion, progress.Candy, progress.CandyEarned, new List<string>(progress.OwnedSkins),
                progress.WornIn(HalloweenSkinSlot.Beam), progress.WornIn(HalloweenSkinSlot.LensFrame), progress.WornIn(HalloweenSkinSlot.Radar),
                progress.IsBurned, progress.IsWelcomed));
        }
    }
}
