using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _idleBackground;
        [SerializeField] private Color _heldBackground;
        [SerializeField] private Color _idleLabel;
        [SerializeField] private Color _heldLabel;

        private int _pointerId = -1;

        public event Action Pressed;

        public event Action Released;

        public bool IsHeld => _pointerId >= 0;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsHeld)
                return;

            _pointerId = eventData.pointerId;
            SetHeldVisual(true);
            Pressed?.Invoke();
        }

        // Pointer-up is delivered to the pressed object even if the finger slid off it, so a drag never leaves BEAM stuck on.
        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
                return;

            Release();
        }

        private void OnDisable()
        {
            if (IsHeld)
                Release();
        }

        private void Release()
        {
            _pointerId = -1;
            SetHeldVisual(false);
            Released?.Invoke();
        }

        private void SetHeldVisual(bool held)
        {
            _background.color = held ? _heldBackground : _idleBackground;
            _label.color = held ? _heldLabel : _idleLabel;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _background = GetComponent<Image>();
            _label = GetComponentInChildren<TMP_Text>(true);
        }
#endif
    }
}
