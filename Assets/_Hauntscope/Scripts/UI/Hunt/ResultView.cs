using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    public sealed class ResultView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _ghostNameLabel;
        [SerializeField] private TMP_Text _rewardLabel;
        [SerializeField] private TMP_Text _timeLabel;
        [SerializeField] private Button _huntAgainButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Color _capturedColor;
        [SerializeField] private Color _escapedColor;
        [SerializeField] private Material _capturedTitleMaterial;
        [SerializeField] private Material _escapedTitleMaterial;

        public event Action HuntAgainClicked;

        public event Action MenuClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetTitle(string text, bool captured)
        {
            _titleLabel.text = text;
            _titleLabel.color = captured ? _capturedColor : _escapedColor;
            _titleLabel.fontSharedMaterial = captured ? _capturedTitleMaterial : _escapedTitleMaterial;
        }

        public void SetGhostName(string text)
        {
            _ghostNameLabel.text = text;
        }

        public void SetReward(string text)
        {
            _rewardLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _rewardLabel.text = text;
        }

        public void SetTime(string text)
        {
            _timeLabel.text = text;
        }

        private void Awake()
        {
            _huntAgainButton.onClick.AddListener(OnHuntAgainClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void OnDestroy()
        {
            _huntAgainButton.onClick.RemoveListener(OnHuntAgainClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void OnHuntAgainClicked()
        {
            HuntAgainClicked?.Invoke();
        }

        private void OnMenuClicked()
        {
            MenuClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _titleLabel = Find<TMP_Text>("Card/Title");
            _ghostNameLabel = Find<TMP_Text>("Card/GhostName");
            _rewardLabel = Find<TMP_Text>("Card/Reward");
            _timeLabel = Find<TMP_Text>("Card/Time");
            _huntAgainButton = Find<Button>("Card/HuntAgainButton");
            _menuButton = Find<Button>("Card/MenuButton");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
