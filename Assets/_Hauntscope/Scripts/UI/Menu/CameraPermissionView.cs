using System;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class CameraPermissionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _bodyLabel;
        [SerializeField] private Typewriter _bodyTypewriter;
        [SerializeField] private Button _allowButton;
        [SerializeField] private TMP_Text _allowLabel;
        [SerializeField] private Button _virtualButton;

        public event Action AllowClicked;

        public event Action VirtualClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetBody(string text)
        {
            _bodyLabel.text = text;
            _bodyTypewriter.Play();
        }

        public void SetAllowLabel(string text)
        {
            _allowLabel.text = text;
        }

        private void Awake()
        {
            _allowButton.onClick.AddListener(OnAllowClicked);
            _virtualButton.onClick.AddListener(OnVirtualClicked);
        }

        private void OnDestroy()
        {
            _allowButton.onClick.RemoveListener(OnAllowClicked);
            _virtualButton.onClick.RemoveListener(OnVirtualClicked);
        }

        private void OnAllowClicked()
        {
            AllowClicked?.Invoke();
        }

        private void OnVirtualClicked()
        {
            VirtualClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _bodyLabel = Find<TMP_Text>("Card/Body");
            _allowButton = Find<Button>("Card/AllowButton");
            _allowLabel = Find<TMP_Text>("Card/AllowButton/Label");
            _virtualButton = Find<Button>("Card/VirtualButton");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
