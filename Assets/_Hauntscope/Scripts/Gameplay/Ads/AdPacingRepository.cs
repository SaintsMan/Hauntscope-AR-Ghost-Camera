using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Ads
{
    public sealed class AdPacingRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "ad_pacing";

        private readonly ISaveService _save;

        public AdPacingRepository(ISaveService save)
        {
            _save = save;
        }

        public AdPacing Load()
        {
            if (!_save.TryLoad<AdPacingDto>(Key, out var dto) || dto.Version < 1 || dto.Version > CurrentVersion)
                return new AdPacing();

            var lastAd = dto.LastAdTicks > 0 ? new DateTime(dto.LastAdTicks, DateTimeKind.Utc) : (DateTime?)null;
            return new AdPacing(dto.HuntsSinceInterstitial, lastAd);
        }

        public void Save(AdPacing pacing)
        {
            _save.Save(Key, new AdPacingDto(CurrentVersion, pacing.HuntsSinceInterstitial, pacing.LastAdAt?.Ticks ?? 0L));
        }
    }
}
