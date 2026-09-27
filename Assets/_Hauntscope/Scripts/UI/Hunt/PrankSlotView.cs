using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // One caught ghost in the prank strip: its Bestiary icon in a frame of its own colour, lit when chosen.
    public sealed class PrankSlotView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Graphic _frame;
        [SerializeField] private Graphic _fill;
        [SerializeField, Range(0f, 1f)] private float _idleFrameAlpha = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _selectedFillAlpha = 0.3f;
        [SerializeField, Min(1f)] private float _selectedScale = 1.12f;

        private Color _rim;

        public event Action Clicked;

        public void Set(Sprite icon, Color rim)
        {
            _icon.sprite = icon;
            _rim = rim;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            _frame.color = new Color(_rim.r, _rim.g, _rim.b, selected ? 1f : _idleFrameAlpha);
            _fill.color = new Color(_rim.r, _rim.g, _rim.b, selected ? _selectedFillAlpha : 0f);
            transform.localScale = Vector3.one * (selected ? _selectedScale : 1f);
        }

        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _button = GetComponent<Button>();
            _icon = transform.Find("Icon")?.GetComponent<Image>();
            _frame = transform.Find("Frame")?.GetComponent<Graphic>();
            _fill = transform.Find("Fill")?.GetComponent<Graphic>();
        }
#endif
    }
}
