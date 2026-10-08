using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The game's own question before Android's: what the reminders are for, then "turn on" or "not now".
    public sealed class NotificationPromptView : MonoBehaviour
    {
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _laterButton;

        public event Action AcceptClicked;
        public event Action LaterClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void Awake()
        {
            _acceptButton.onClick.AddListener(OnAcceptClicked);
            _laterButton.onClick.AddListener(OnLaterClicked);
        }

        private void OnDestroy()
        {
            _acceptButton.onClick.RemoveListener(OnAcceptClicked);
            _laterButton.onClick.RemoveListener(OnLaterClicked);
        }

        private void OnAcceptClicked()
        {
            AcceptClicked?.Invoke();
        }

        private void OnLaterClicked()
        {
            LaterClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var accept = transform.Find("Card/AcceptButton");
            _acceptButton = accept != null ? accept.GetComponent<Button>() : null;
            var later = transform.Find("Card/LaterButton");
            _laterButton = later != null ? later.GetComponent<Button>() : null;
        }
#endif
    }
}
