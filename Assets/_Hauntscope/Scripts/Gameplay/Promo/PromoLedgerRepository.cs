using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Promo
{
    public sealed class PromoLedgerRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "promo_granted";

        private readonly ISaveService _save;

        public PromoLedgerRepository(ISaveService save)
        {
            _save = save;
        }

        public PromoLedger Load()
        {
            if (!_save.TryLoad<PromoLedgerDto>(Key, out var dto) || dto.Version < 1 || dto.Version > CurrentVersion)
                return new PromoLedger();

            return new PromoLedger(dto.Codes);
        }

        public void Save(PromoLedger ledger)
        {
            _save.Save(Key, new PromoLedgerDto(CurrentVersion, new List<string>(ledger.Codes)));
        }
    }
}
