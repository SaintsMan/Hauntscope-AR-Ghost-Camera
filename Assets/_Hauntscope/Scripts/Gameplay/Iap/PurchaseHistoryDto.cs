using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Iap
{
    [Serializable]
    public sealed class PurchaseHistoryDto
    {
        [SerializeField] private int _version;
        [SerializeField] private string[] _owned;
        [SerializeField] private string[] _transactions;
        [SerializeField] private bool _premium;
        [SerializeField] private long _starterOfferStart;
        [SerializeField] private bool _starterOfferAnnounced;
        [SerializeField] private int _premiumOfferDay;
        // Added without a version bump: older saves read 0, never shown.
        [SerializeField] private int _kitOfferDay;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public PurchaseHistoryDto()
        {
        }

        public PurchaseHistoryDto(int version, string[] owned, string[] transactions, bool premium, long starterOfferStart,
            bool starterOfferAnnounced, int premiumOfferDay, int kitOfferDay)
        {
            _kitOfferDay = kitOfferDay;
            _version = version;
            _owned = owned;
            _transactions = transactions;
            _premium = premium;
            _starterOfferStart = starterOfferStart;
            _starterOfferAnnounced = starterOfferAnnounced;
            _premiumOfferDay = premiumOfferDay;
        }

        public int Version => _version;
        public string[] Owned => _owned;
        public string[] Transactions => _transactions;
        public bool Premium => _premium;
        public long StarterOfferStart => _starterOfferStart;
        public bool StarterOfferAnnounced => _starterOfferAnnounced;
        public int PremiumOfferDay => _premiumOfferDay;
        public int KitOfferDay => _kitOfferDay;
    }
}
