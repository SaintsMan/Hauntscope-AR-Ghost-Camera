using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // "Battery dead" card: a draining ring counts down the seconds before the ghost gets away.
    public sealed class EmergencyChargeView : MonoBehaviour
    {
        [SerializeField] private Image _ring;
        [SerializeField] private TMP_Text _secondsLabel;
        [SerializeField] private Button _spareButton;
        [SerializeField] private TMP_Text _spareLabel;
        [SerializeField] private Button _adButton;
        [SerializeField] private CanvasGroup _adGroup;
        [SerializeField] private Button _giveUpButton;
        [SerializeField] private RectTransform _card;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.4f;

        private int _shownSeconds = -1;

        public event Action SpareClicked;

        public event Action AdClicked;

        public event Action GiveUpClicked;

        public void SetVisible(bool visible)
        {
            if (visible && !gameObject.activeSelf)
                _shownSeconds = -1;
            gameObject.SetActive(visible);
        }

        public void SetCountdown(float fraction, int seconds, Func<int, string> format)
        {
            _ring.fillAmount = fraction;
            if (seconds == _shownSeconds)
                return;

            _shownSeconds = seconds;
            _secondsLabel.text = format(seconds);
            if (!isActiveAndEnabled)
                return;

            // Every second the card jolts, like the camera's alarm, so the pressure is felt as well as read.
            _secondsLabel.transform.DOKill(true);
            _secondsLabel.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 6).Ui(gameObject);
            _card.DOKill(true);
            _card.DOShakeAnchorPos(0.2f, 6f, 20, 90f, false, true).Ui(gameObject);
        }

        public void SetSpare(bool available, string label)
        {
            _spareButton.gameObject.SetActive(available);
            _spareLabel.text = label;
        }

        public void SetAd(bool visible, bool ready)
        {
            _adButton.gameObject.SetActive(visible);
            _adButton.interactable = ready;
            _adGroup.alpha = ready ? 1f : _disabledAlpha;
        }

        private void Awake()
        {
            _spareButton.onClick.AddListener(OnSpareClicked);
            _adButton.onClick.AddListener(OnAdClicked);
            _giveUpButton.onClick.AddListener(OnGiveUpClicked);
        }

        private void OnDestroy()
        {
            _spareButton.onClick.RemoveListener(OnSpareClicked);
            _adButton.onClick.RemoveListener(OnAdClicked);
            _giveUpButton.onClick.RemoveListener(OnGiveUpClicked);
        }

        private void OnSpareClicked()
        {
            SpareClicked?.Invoke();
        }

        private void OnAdClicked()
        {
            AdClicked?.Invoke();
        }

        private void OnGiveUpClicked()
        {
            GiveUpClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _card = Find<RectTransform>("Card");
            _ring = Find<Image>("Card/Countdown/Ring");
            _secondsLabel = Find<TMP_Text>("Card/Countdown/Seconds");
            _spareButton = Find<Button>("Card/SpareButton");
            _spareLabel = Find<TMP_Text>("Card/SpareButton/Row/Label");
            _adButton = Find<Button>("Card/AdButton");
            _adGroup = Find<CanvasGroup>("Card/AdButton");
            _giveUpButton = Find<Button>("Card/GiveUpButton");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
