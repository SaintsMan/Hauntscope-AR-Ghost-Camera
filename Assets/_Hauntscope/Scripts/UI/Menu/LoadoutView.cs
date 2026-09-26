using UnityEngine;

namespace Hauntscope.UI.Menu
{
    public sealed class LoadoutView : MonoBehaviour
    {
        [SerializeField] private RectTransform _slotsRoot;
        [SerializeField] private LoadoutSlotView _slotPrefab;

        public LoadoutSlotView AddSlot()
        {
            return Instantiate(_slotPrefab, _slotsRoot);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var slots = transform.Find("Slots");
            _slotsRoot = slots != null ? (RectTransform)slots : null;
        }
#endif
    }
}
