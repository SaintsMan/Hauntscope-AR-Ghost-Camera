using UnityEngine;

namespace Hauntscope.Gameplay.Shift
{
    // A perk on offer at a shift break: its card (name, what it does, icon) and the effect it creates.
    public abstract class ShiftPerkData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private Sprite _icon;

        public string Id => _id;

        public string NameKey => _nameKey;

        public string DescriptionKey => _descriptionKey;

        public Sprite Icon => _icon;

        public abstract IShiftPerk CreatePerk();
    }
}
