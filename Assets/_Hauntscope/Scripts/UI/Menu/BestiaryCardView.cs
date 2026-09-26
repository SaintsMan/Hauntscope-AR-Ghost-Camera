using System;
using Hauntscope.Gameplay.Research;
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
        [SerializeField] private Color _sightedStampColor = new Color(1f, 0.71f, 0.28f, 1f);
        [SerializeField] private Color _declassifiedStampColor = new Color(1f, 0.82f, 0.4f, 1f);
        [SerializeField, Range(0f, 1f)] private float _sightedTint = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _sightedBorderAlpha = 0.55f;

        public event Action Clicked;

        // A ghost the agency has only glimpsed stays a silhouette with a trace of its colour; a caught one is shown whole.
        public void SetEntry(Sprite icon, Color accent, string ghostName, string count, string stamp, ResearchLevel level)
        {
            var known = level >= ResearchLevel.Captured;
            _icon.sprite = icon;
            _icon.color = known ? Color.white
                : level == ResearchLevel.Sighted ? Color.Lerp(_silhouetteColor, accent, _sightedTint) : _silhouetteColor;

            var border = accent;
            if (level == ResearchLevel.Sighted)
                border.a *= _sightedBorderAlpha;
            _border.color = level == ResearchLevel.Unknown ? _unknownColor : border;

            _nameLabel.text = ghostName;
            _nameLabel.color = level == ResearchLevel.Unknown ? _unknownColor : Color.white;
            _countLabel.text = count;
            SetStamp(stamp, StampColor(level));
        }

        private Color StampColor(ResearchLevel level)
        {
            switch (level)
            {
                case ResearchLevel.Sighted:
                    return _sightedStampColor;
                case ResearchLevel.Captured:
                    return _capturedStampColor;
                case ResearchLevel.Declassified:
                    return _declassifiedStampColor;
                default:
                    return _unknownStampColor;
            }
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
