using UnityEngine;

namespace Hauntscope.Gameplay.Store
{
    [CreateAssetMenu(fileName = "Laser", menuName = "Hauntscope/Laser")]
    public sealed class LaserData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private Sprite _icon;
        [SerializeField, Min(0)] private int _price;
        [SerializeField] private Color _beamColor = Color.red;
        [SerializeField] private Color _lockedColor = Color.yellow;
        [SerializeField] private HuntModifierSet _modifiers = new HuntModifierSet();
        [SerializeField, Range(0, 5)] private int _powerRating = 2;
        [SerializeField, Range(0, 5)] private int _holdRating = 2;
        [SerializeField, Range(0, 5)] private int _economyRating = 2;

        public string Id => _id;

        public string NameKey => _nameKey;

        public string DescriptionKey => _descriptionKey;

        public Sprite Icon => _icon;

        public int Price => _price;

        public Color BeamColor => _beamColor;

        public Color LockedColor => _lockedColor;

        public HuntModifierSet Modifiers => _modifiers;

        // Shop card ratings (LED bars): authored next to the modifiers, since "hold" mixes ring width, decay and slowing.
        public int PowerRating => _powerRating;

        public int HoldRating => _holdRating;

        public int EconomyRating => _economyRating;
    }
}
