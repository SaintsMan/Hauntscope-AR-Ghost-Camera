using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    // A photo full-screen with the two things people do with a good one: share it or keep it.
    public sealed class PhotoViewerView : MonoBehaviour
    {
        [SerializeField] private RawImage _photo;
        [SerializeField] private AspectRatioFitter _photoFitter;
        [SerializeField] private TMP_Text _caption;
        [SerializeField] private Button _shareButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private TMP_Text _saveLabel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backdropButton;

        public event Action ShareClicked;

        public event Action SaveClicked;

        public event Action CloseClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetPhoto(Texture photo, string caption)
        {
            _photo.texture = photo;
            if (photo != null)
                _photoFitter.aspectRatio = photo.width / (float)photo.height;
            _caption.text = caption;
        }

        public void SetSave(bool visible, bool interactable, string label)
        {
            _saveButton.gameObject.SetActive(visible);
            _saveButton.interactable = interactable;
            _saveLabel.text = label;
        }

        private void Awake()
        {
            _shareButton.onClick.AddListener(OnShare);
            _saveButton.onClick.AddListener(OnSave);
            _closeButton.onClick.AddListener(OnClose);
            _backdropButton.onClick.AddListener(OnClose);
        }

        private void OnDestroy()
        {
            _shareButton.onClick.RemoveListener(OnShare);
            _saveButton.onClick.RemoveListener(OnSave);
            _closeButton.onClick.RemoveListener(OnClose);
            _backdropButton.onClick.RemoveListener(OnClose);
        }

        private void OnShare()
        {
            ShareClicked?.Invoke();
        }

        private void OnSave()
        {
            SaveClicked?.Invoke();
        }

        private void OnClose()
        {
            CloseClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _photo = GetComponentInChildren<RawImage>(true);
            _photoFitter = GetComponentInChildren<AspectRatioFitter>(true);
            _caption = transform.Find("Caption")?.GetComponent<TMP_Text>();
            _shareButton = transform.Find("ShareButton")?.GetComponent<Button>();
            _saveButton = transform.Find("SaveButton")?.GetComponent<Button>();
            _saveLabel = transform.Find("SaveButton/Label")?.GetComponent<TMP_Text>();
            _closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
            _backdropButton = transform.Find("Backdrop")?.GetComponent<Button>();
        }
#endif
    }
}
