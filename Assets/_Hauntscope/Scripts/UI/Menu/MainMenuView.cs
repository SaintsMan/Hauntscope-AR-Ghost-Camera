using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _bestiaryButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _ectoplasmLabel;
        [SerializeField] private TMP_Text _versionLabel;
        [SerializeField] private TMP_Text _timestampLabel;
        [SerializeField] private RectTransform _ectoplasmIcon;
        [SerializeField, Min(0.05f)] private float _countDuration = 0.9f;

        private int _shownEctoplasm = -1;

        public event Action StartClicked;

        public event Action BestiaryClicked;

        public event Action SettingsClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        // The balance counts up to a new value, so ectoplasm earned in a hunt is noticed on the way back.
        public void SetEctoplasm(int amount)
        {
            if (_shownEctoplasm < 0 || !isActiveAndEnabled)
            {
                ShowEctoplasm(amount);
                return;
            }

            DOTween.To(() => _shownEctoplasm, ShowEctoplasm, amount, _countDuration).SetEase(Ease.OutCubic).Ui(gameObject);
            _ectoplasmIcon.DOPunchScale(Vector3.one * 0.35f, _countDuration, 6).Ui(gameObject);
        }

        public void SetTimestamp(string text)
        {
            _timestampLabel.text = text;
        }

        public void SetVersion(string text)
        {
            _versionLabel.text = text;
        }

        private void Awake()
        {
            _startButton.onClick.AddListener(OnStartClicked);
            _bestiaryButton.onClick.AddListener(OnBestiaryClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _bestiaryButton.onClick.RemoveListener(OnBestiaryClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        private void OnStartClicked()
        {
            StartClicked?.Invoke();
        }

        private void OnBestiaryClicked()
        {
            BestiaryClicked?.Invoke();
        }

        private void OnSettingsClicked()
        {
            SettingsClicked?.Invoke();
        }

        private void ShowEctoplasm(int amount)
        {
            _shownEctoplasm = amount;
            _ectoplasmLabel.text = amount.ToString();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _startButton = Find<Button>("Buttons/StartButton");
            _bestiaryButton = Find<Button>("Buttons/BestiaryButton");
            _settingsButton = Find<Button>("Buttons/SettingsButton");
            _ectoplasmLabel = Find<TMP_Text>("Ectoplasm/Amount");
            _versionLabel = Find<TMP_Text>("Version");
            _timestampLabel = Find<TMP_Text>("Timestamp");
            _ectoplasmIcon = Find<RectTransform>("Ectoplasm/Icon");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
