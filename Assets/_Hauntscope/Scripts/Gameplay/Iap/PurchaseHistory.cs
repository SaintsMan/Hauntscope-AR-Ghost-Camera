using System;
using System.Collections.Generic;

namespace Hauntscope.Gameplay.Iap
{
    // What the player has paid for and where the paid offers stand. Ownership survives a reinstall through the store's
    // own records; this save keeps the game from granting one order twice.
    public sealed class PurchaseHistory
    {
        private const int KeptTransactions = 64;

        private readonly HashSet<string> _owned;
        private readonly List<string> _transactions;

        public PurchaseHistory()
            : this(Array.Empty<string>(), Array.Empty<string>(), false, 0L, false, 0, 0)
        {
        }

        public PurchaseHistory(IEnumerable<string> owned, IEnumerable<string> transactions, bool isPremium,
            long starterOfferStartTicks, bool starterOfferAnnounced, int premiumOfferShownDay, int kitOfferShownDay)
        {
            _owned = new HashSet<string>(owned);
            _transactions = new List<string>(transactions);
            IsPremium = isPremium;
            StarterOfferStartTicks = starterOfferStartTicks;
            StarterOfferAnnounced = starterOfferAnnounced;
            PremiumOfferShownDay = premiumOfferShownDay;
            KitOfferShownDay = kitOfferShownDay;
        }

        public event Action Changed;

        public IReadOnlyCollection<string> Owned => _owned;

        public IReadOnlyList<string> Transactions => _transactions;

        public bool IsPremium { get; private set; }

        // UTC ticks of the moment the rookie kit offer opened; 0 while it has not.
        public long StarterOfferStartTicks { get; private set; }

        public bool StarterOfferAnnounced { get; private set; }

        public int PremiumOfferShownDay { get; private set; }

        public int KitOfferShownDay { get; private set; }

        public bool Owns(string productId)
        {
            return _owned.Contains(productId);
        }

        public bool WasDelivered(string transactionId)
        {
            return !string.IsNullOrEmpty(transactionId) && _transactions.Contains(transactionId);
        }

        public void RecordDelivery(string productId, bool owned, string transactionId)
        {
            if (owned)
                _owned.Add(productId);
            if (!string.IsNullOrEmpty(transactionId) && !_transactions.Contains(transactionId))
            {
                _transactions.Add(transactionId);
                if (_transactions.Count > KeptTransactions)
                    _transactions.RemoveAt(0);
            }

            Changed?.Invoke();
        }

        public void SetPremium()
        {
            if (IsPremium)
                return;

            IsPremium = true;
            Changed?.Invoke();
        }

        public void StartStarterOffer(DateTime utcNow)
        {
            if (StarterOfferStartTicks != 0L)
                return;

            StarterOfferStartTicks = utcNow.Ticks;
            Changed?.Invoke();
        }

        public void MarkStarterOfferAnnounced()
        {
            StarterOfferAnnounced = true;
            Changed?.Invoke();
        }

        public void MarkKitOfferShown(int day)
        {
            KitOfferShownDay = day;
            Changed?.Invoke();
        }

        public void MarkPremiumOfferShown(int day)
        {
            PremiumOfferShownDay = day;
            Changed?.Invoke();
        }
    }
}
