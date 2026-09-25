using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class BestiaryCardView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _border;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _countLabel;
        [SerializeField] private TMP_Text _stampLabel;
        [SerializeField] private Graphic _stampFrame;
        [SerializeField] private Color _silhouetteColor;
        [SerializeField] private Color _unknownColor;
        [SerializeField] private Color _unknownStampColor;
        [SerializeField] private Color _capturedStampColor;

        public event Action Clicked;

        public void SetCaptured(Sprite icon, Color accent, string ghostName, string count, string stamp)
        {
            _icon.sprite = icon;
            _icon.color = Color.white;
            _border.color = accent;
            _nameLabel.text = ghostName;
            _nameLabel.color = Color.white;
            _countLabel.text = count;
            SetStamp(stamp, _capturedStampColor);
        }

        public void SetUnknown(Sprite icon, string ghostName, string stamp)
        {
            _icon.sprite = icon;
            _icon.color = _silhouetteColor;
            _border.color = _unknownColor;
            _nameLabel.text = ghostName;
            _nameLabel.color = _unknownColor;
            _countLabel.text = string.Empty;
            SetStamp(stamp, _unknownStampColor);
        }

        private void SetStamp(string text, Color color)
        {
            _stampLabel.text = text;
            _stampLabel.color = color;
            _stampFrame.color = color;
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
            _icon = Find<Image>("Icon");
            _border = Find<Image>("Border");
            _nameLabel = Find<TMP_Text>("Name");
            _countLabel = Find<TMP_Text>("Count");
            _stampLabel = Find<TMP_Text>("Stamp/Label");
            _stampFrame = Find<Graphic>("Stamp");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
