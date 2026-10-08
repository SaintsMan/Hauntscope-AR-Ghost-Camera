using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Iap
{
    // A product sold for real money. Each kind decides what it grants, so the store never asks what it is selling.
    public abstract class IapProductData : ScriptableObject
    {
        [SerializeField] private string _productId;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _accent = Color.white;
        [SerializeField] private string _badgeKey;
        [SerializeField, Min(0f)] private float _referencePriceUsd = 0.99f;

        // The Google Play product id: permanent once created in Play Console.
        public string ProductId => _productId;
        public string NameKey => _nameKey;
        public string DescriptionKey => _descriptionKey;
        public Sprite Icon => _icon;
        public Color Accent => _accent;
        // A ribbon over the card ("BEST VALUE"); empty for none.
        public string BadgeKey => _badgeKey;
        public decimal ReferencePriceUsd => (decimal)_referencePriceUsd;

        public abstract bool IsConsumable { get; }

        // A laser that only this product unlocks, so the laser list can sell it for money. A laser that merely comes
        // in a box (the rookie kit's tether) is still sold for ectoplasm and does not count.
        public virtual LaserData ExclusiveLaser => null;

        public abstract void Grant(IapGrant grant);
    }
}
