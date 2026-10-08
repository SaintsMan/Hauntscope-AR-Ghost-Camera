using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The full version at the top of the supplies tab: a gold case file with the perks listed and a light sweeping across
    // it, so it reads as the one thing worth having. Once bought it turns into a stamped "active" licence.
    public sealed class PremiumCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _perksLabel;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TMP_Text _priceLabel;
        [SerializeField] private GameObject _ownedStamp;
        [SerializeField] private TMP_Text _ownedLabel;
        [SerializeField] private RectTransform _icon;
        [SerializeField] private RectTransform _shine;
        [SerializeField] private float _shineTravel = 1400f;
        [SerializeField, Min(0.1f)] private float _shinePeriod = 3.2f;
        [SerializeField, Min(0.1f)] private float _iconBobPeriod = 2.4f;
        [SerializeField] private float _iconBob = 10f;

        private Vector2 _shineStart;
        private Vector2 _iconStart;
        private bool _captured;

        public event Action BuyClicked;

        public void SetContent(string title, string perks)
        {
            _titleLabel.text = title;
            _perksLabel.text = perks;
        }

        public void SetPrice(string label, bool available)
        {
            _priceLabel.text = label;
            _buyButton.interactable = available;
        }

        public void SetOwned(bool owned, string label)
        {
            _buyButton.gameObject.SetActive(!owned);
            _ownedStamp.SetActive(owned);
            _ownedLabel.text = label;
        }

        public void PlayPurchased()
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.05f, 0.5f, 6).Ui(gameObject);
            _ownedStamp.transform.localScale = Vector3.one * 2.2f;
            _ownedStamp.transform.DOScale(1f, 0.32f).SetEase(Ease.InQuad).Ui(gameObject);
        }

        public void PlayDenied()
        {
            _buyButton.transform.DOKill(true);
            _buyButton.transform.DOShakePosition(0.35f, new Vector3(14f, 0f, 0f), 18, 0f).Ui(gameObject);
        }

        private void OnEnable()
        {
            if (!_captured)
            {
                _captured = true;
                _shineStart = _shine.anchoredPosition;
                _iconStart = _icon.anchoredPosition;
            }

            _shine.anchoredPosition = _shineStart;
            _shine.DOAnchorPosX(_shineStart.x + _shineTravel, _shinePeriod).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart)
                .Ui(gameObject);
            _icon.anchoredPosition = _iconStart;
            _icon.DOAnchorPosY(_iconStart.y + _iconBob, _iconBobPeriod * 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .Ui(gameObject);
        }

        private void Awake()
        {
            _buyButton.onClick.AddListener(OnBuyClicked);
        }

        private void OnDestroy()
        {
            _buyButton.onClick.RemoveListener(OnBuyClicked);
        }

        private void OnBuyClicked()
        {
            BuyClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _titleLabel = Find<TMP_Text>("Title");
            _perksLabel = Find<TMP_Text>("Perks");
            _buyButton = Find<Button>("Buy");
            _priceLabel = Find<TMP_Text>("Buy/Label");
            var stamp = transform.Find("Owned");
            _ownedStamp = stamp != null ? stamp.gameObject : null;
            _ownedLabel = Find<TMP_Text>("Owned/Label");
            _icon = Find<RectTransform>("Icon");
            _shine = Find<RectTransform>("Mask/Shine");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
