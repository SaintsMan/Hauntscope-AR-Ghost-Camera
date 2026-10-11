using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The Bureau's note that this build is too old: "update" or "not now".
    public sealed class UpdateNoticeView : MonoBehaviour
    {
        [SerializeField] private Button _updateButton;
        [SerializeField] private Button _laterButton;

        public event Action UpdateClicked;
        public event Action LaterClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void Awake()
        {
            _updateButton.onClick.AddListener(OnUpdateClicked);
            _laterButton.onClick.AddListener(OnLaterClicked);
        }

        private void OnDestroy()
        {
            _updateButton.onClick.RemoveListener(OnUpdateClicked);
            _laterButton.onClick.RemoveListener(OnLaterClicked);
        }

        private void OnUpdateClicked()
        {
            UpdateClicked?.Invoke();
        }

        private void OnLaterClicked()
        {
            LaterClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var update = transform.Find("Card/UpdateButton");
            _updateButton = update != null ? update.GetComponent<Button>() : null;
            var later = transform.Find("Card/LaterButton");
            _laterButton = later != null ? later.GetComponent<Button>() : null;
        }
#endif
    }
}
