using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class VirtualRoomNoticeView : MonoBehaviour
    {
        [SerializeField] private Button _okButton;

        public event Action OkClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void Awake()
        {
            _okButton.onClick.AddListener(OnOkClicked);
        }

        private void OnDestroy()
        {
            _okButton.onClick.RemoveListener(OnOkClicked);
        }

        private void OnOkClicked()
        {
            OkClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var child = transform.Find("Card/OkButton");
            _okButton = child != null ? child.GetComponent<Button>() : null;
        }
#endif
    }
}
