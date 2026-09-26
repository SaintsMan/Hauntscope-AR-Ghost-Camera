using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The ration's icon in the menu corner, with a dot while today's frame is still waiting.
    public sealed class CalendarButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _badge;

        public event Action Clicked;

        public void SetBadge(bool visible)
        {
            _badge.SetActive(visible);
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
            var badge = transform.Find("Badge");
            _badge = badge != null ? badge.gameObject : null;
        }
#endif
    }
}
