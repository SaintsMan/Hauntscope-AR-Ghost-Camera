using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Iap
{
    public sealed class PurchaseHistoryRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "purchases";

        private readonly ISaveService _save;

        public PurchaseHistoryRepository(ISaveService save)
        {
            _save = save;
        }

        public PurchaseHistory Load()
        {
            if (!_save.TryLoad<PurchaseHistoryDto>(Key, out var dto) || dto.Version < 1 || dto.Version > CurrentVersion)
                return new PurchaseHistory();

            return new PurchaseHistory(dto.Owned ?? Array.Empty<string>(), dto.Transactions ?? Array.Empty<string>(),
                dto.Premium, dto.StarterOfferStart, dto.StarterOfferAnnounced, dto.PremiumOfferDay);
        }

        public void Save(PurchaseHistory history)
        {
            _save.Save(Key, new PurchaseHistoryDto(CurrentVersion, ToArray(history.Owned), ToArray(history.Transactions),
                history.IsPremium, history.StarterOfferStartTicks, history.StarterOfferAnnounced, history.PremiumOfferShownDay));
        }

        private static string[] ToArray(IReadOnlyCollection<string> values)
        {
            var array = new string[values.Count];
            var i = 0;
            foreach (var value in values)
                array[i++] = value;
            return array;
        }
    }
}
