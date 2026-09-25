using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    public sealed class ToggleButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _offBackground;
        [SerializeField] private Color _onBackground;
        [SerializeField] private Color _offLabel;
        [SerializeField] private Color _onLabel;

        public event Action Clicked;

        public void SetOn(bool isOn)
        {
            _background.color = isOn ? _onBackground : _offBackground;
            _label.color = isOn ? _onLabel : _offLabel;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke();
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
