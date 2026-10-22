using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Season
{
    // One thing the candy buys (GDD 5.42): a repaint of the beam, the lens frame or the radar. Bought skins stay for good.
    [Serializable]
    public sealed class HalloweenSkin
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private HalloweenSkinSlot _slot;
        [SerializeField, Min(0)] private int _price = 10;
        [SerializeField] private Color _color = new Color(1f, 0.46f, 0.1f, 1f);
        [SerializeField] private Sprite _icon;
        [SerializeField] private Sprite _overlay;

        public HalloweenSkin()
        {
        }

        public HalloweenSkin(string id, HalloweenSkinSlot slot, int price, Color color)
        {
            _id = id;
            _slot = slot;
            _price = price;
            _color = color;
        }

        public string Id => _id;

        public string NameKey => _nameKey;

        public string DescriptionKey => _descriptionKey;

        public HalloweenSkinSlot Slot => _slot;

        public int Price => _price;

        // The beam and the radar are tinted with it; the lens frame uses it for the glow behind the cobwebs.
        public Color Color => _color;

        public Sprite Icon => _icon;

        // The lens frame's cobwebs, drawn over the viewfinder.
        public Sprite Overlay => _overlay;
    }
}
