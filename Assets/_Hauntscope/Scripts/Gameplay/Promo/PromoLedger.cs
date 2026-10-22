using System.Collections.Generic;

namespace Hauntscope.Gameplay.Promo
{
    // The codes this install has already been paid for. The server answers a quick retry with "repeat" and the same
    // rewards, so only the ledger keeps a lost-then-repeated answer from paying twice.
    public sealed class PromoLedger
    {
        private readonly HashSet<string> _codes;

        public PromoLedger()
            : this(null)
        {
        }

        public PromoLedger(IEnumerable<string> codes)
        {
            _codes = codes != null ? new HashSet<string>(codes) : new HashSet<string>();
        }

        public IReadOnlyCollection<string> Codes => _codes;

        public bool Contains(string code)
        {
            return _codes.Contains(code);
        }

        public bool Add(string code)
        {
            return !string.IsNullOrEmpty(code) && _codes.Add(code);
        }
    }
}
