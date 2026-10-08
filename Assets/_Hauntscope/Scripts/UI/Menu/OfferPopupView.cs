using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // A paid offer the menu opens by itself: the rookie kit when it starts, the full version now and then.
    public sealed class OfferPopupView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Graphic _iconGlow;
        [SerializeField] private Graphic _frame;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _bodyLabel;
        [SerializeField] private GameObject _ribbon;
        [SerializeField] private TMP_Text _ribbonLabel;
        [SerializeField] private TMP_Text _timerLabel;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TMP_Text _priceLabel;
        [SerializeField] private Button _laterButton;
        [SerializeField, Range(0f, 1f)] private float _glowAlpha = 0.4f;

        public event Action BuyClicked;
        public event Action LaterClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetContent(Sprite icon, Color accent, string title, string body, string ribbon)
        {
            _icon.sprite = icon;
            _icon.color = accent;
            var glow = accent;
            glow.a = _glowAlpha;
            _iconGlow.color = glow;
            _frame.color = accent;
            _titleLabel.text = title;
            _titleLabel.color = accent;
            _bodyLabel.text = body;
            _ribbon.SetActive(!string.IsNullOrEmpty(ribbon));
            _ribbonLabel.text = ribbon;
        }

        // Empty hides the clock: the full version never runs out.
        public void SetTimer(string text)
        {
            _timerLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _timerLabel.text = text;
        }

        public void SetPrice(string label, bool available)
        {
            _priceLabel.text = label;
            _buyButton.interactable = available;
        }

        public void PlayDenied()
        {
            _buyButton.transform.DOKill(true);
            _buyButton.transform.DOShakePosition(0.35f, new Vector3(14f, 0f, 0f), 18, 0f).Ui(gameObject);
        }

        private void Awake()
        {
            _buyButton.onClick.AddListener(OnBuyClicked);
            _laterButton.onClick.AddListener(OnLaterClicked);
        }

        private void OnDestroy()
        {
            _buyButton.onClick.RemoveListener(OnBuyClicked);
            _laterButton.onClick.RemoveListener(OnLaterClicked);
        }

        private void OnBuyClicked()
        {
            BuyClicked?.Invoke();
        }

        private void OnLaterClicked()
        {
            LaterClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _icon = Find<Image>("Card/Icon");
            _iconGlow = Find<Graphic>("Card/IconGlow");
            _frame = Find<Graphic>("Card/Border");
            _titleLabel = Find<TMP_Text>("Card/Title");
            _bodyLabel = Find<TMP_Text>("Card/Body");
            var ribbon = transform.Find("Card/Ribbon");
            _ribbon = ribbon != null ? ribbon.gameObject : null;
            _ribbonLabel = Find<TMP_Text>("Card/Ribbon/Label");
            _timerLabel = Find<TMP_Text>("Card/Timer");
            _buyButton = Find<Button>("Card/BuyButton");
            _priceLabel = Find<TMP_Text>("Card/BuyButton/Label");
            _laterButton = Find<Button>("Card/LaterButton");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
