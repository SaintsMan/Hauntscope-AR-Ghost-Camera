using UnityEngine;

namespace Hauntscope.Gameplay.Store
{
    // A consumable sold in stacks; each kind decides what it does in a hunt.
    public abstract class GearData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _accent = Color.white;
        [SerializeField, Min(0)] private int _price;
        [SerializeField, Min(1)] private int _maxStack = 9;

        public string Id => _id;

        public string NameKey => _nameKey;

        public string DescriptionKey => _descriptionKey;

        public Sprite Icon => _icon;

        public Color Accent => _accent;

        public int Price => _price;

        public int MaxStack => _maxStack;
    }
}
