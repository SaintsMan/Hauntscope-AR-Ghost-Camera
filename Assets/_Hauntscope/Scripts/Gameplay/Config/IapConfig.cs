using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Iap;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // Paid products and the moments the game offers them (GDD 5.37).
    [Serializable]
    public sealed class IapConfig
    {
        [SerializeField] private FullVersionData _fullVersion;
        [SerializeField] private SupplyBundleData _starterPack;
        [SerializeField] private EctoplasmPackData[] _ectoplasmPacks = Array.Empty<EctoplasmPackData>();
        [SerializeField] private IapProductData[] _supplies = Array.Empty<IapProductData>();
        [SerializeField, Min(0)] private int _starterOfferAfterHunts = 2;
        [SerializeField, Min(1f)] private float _starterOfferHours = 48f;
        [SerializeField, Min(0)] private int _starterOfferFallbackHunts = 2;
        [SerializeField] private SupplyBundleData _kitOffer;
        [SerializeField, Min(1)] private int _kitOfferEveryDays = 2;
        [SerializeField, Min(0)] private int _premiumOfferAfterHunts = 6;
        [SerializeField, Min(1)] private int _premiumOfferEveryDays = 3;
        [SerializeField, Min(0f)] private float _topUpDelay = 0.45f;

        public IapConfig()
        {
        }

        public IapConfig(FullVersionData fullVersion, SupplyBundleData starterPack, EctoplasmPackData[] ectoplasmPacks,
            IapProductData[] supplies, int starterOfferAfterHunts = 2, float starterOfferHours = 48f,
            int premiumOfferAfterHunts = 6, int premiumOfferEveryDays = 3, SupplyBundleData kitOffer = null,
            int starterOfferFallbackHunts = 2, int kitOfferEveryDays = 2)
        {
            _kitOffer = kitOffer;
            _starterOfferFallbackHunts = starterOfferFallbackHunts;
            _kitOfferEveryDays = kitOfferEveryDays;
            _fullVersion = fullVersion;
            _starterPack = starterPack;
            _ectoplasmPacks = ectoplasmPacks;
            _supplies = supplies;
            _starterOfferAfterHunts = starterOfferAfterHunts;
            _starterOfferHours = starterOfferHours;
            _premiumOfferAfterHunts = premiumOfferAfterHunts;
            _premiumOfferEveryDays = premiumOfferEveryDays;
        }

        public FullVersionData FullVersion => _fullVersion;
        public SupplyBundleData StarterPack => _starterPack;
        public IReadOnlyList<EctoplasmPackData> EctoplasmPacks => _ectoplasmPacks;
        // Everything else sold for money: the field kit, the lasers.
        public IReadOnlyList<IapProductData> Supplies => _supplies;
        // The rookie kit opens after this many hunts, once the player knows what gear is for.
        public int StarterOfferAfterHunts => _starterOfferAfterHunts;
        public float StarterOfferHours => _starterOfferHours;
        // A player who catches every ghost still hears about the kit this many hunts later.
        public int StarterOfferFallbackHunts => _starterOfferFallbackHunts;
        // Pitched after a ghost escaped while no spare battery was left.
        public SupplyBundleData KitOffer => _kitOffer;
        public int KitOfferEveryDays => _kitOfferEveryDays;
        public int PremiumOfferAfterHunts => _premiumOfferAfterHunts;
        public int PremiumOfferEveryDays => _premiumOfferEveryDays;
        // A purchase refused for lack of ectoplasm turns the shop to the ectoplasm packs after this pause.
        public float TopUpDelay => _topUpDelay;

        public void CollectProducts(List<IapProductData> result)
        {
            Add(result, _fullVersion);
            Add(result, _starterPack);
            foreach (var pack in _ectoplasmPacks)
                Add(result, pack);
            foreach (var product in _supplies)
                Add(result, product);
        }

        private static void Add(List<IapProductData> result, IapProductData product)
        {
            if (product != null)
                result.Add(product);
        }
    }
}
