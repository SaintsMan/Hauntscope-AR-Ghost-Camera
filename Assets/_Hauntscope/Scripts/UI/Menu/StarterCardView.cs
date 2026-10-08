using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The rookie kit while its offer runs: what is inside, a ribbon with the saving and a clock counting down.
    public sealed class StarterCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _contentsLabel;
        [SerializeField] private TMP_Text _ribbonLabel;
        [SerializeField] private TMP_Text _timerLabel;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TMP_Text _priceLabel;
        [SerializeField] private RectTransform _ribbon;
        [SerializeField] private float _ribbonPulse = 1.08f;
        [SerializeField, Min(0.1f)] private float _ribbonPeriod = 0.9f;

        public event Action BuyClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetContent(string title, string contents, string ribbon)
        {
            _titleLabel.text = title;
            _contentsLabel.text = contents;
            _ribbonLabel.text = ribbon;
        }

        public void SetTimer(string text)
        {
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

        private void OnEnable()
        {
            _ribbon.localScale = Vector3.one;
            _ribbon.DOScale(_ribbonPulse, _ribbonPeriod * 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Ui(gameObject);
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
            _contentsLabel = Find<TMP_Text>("Contents");
            _ribbon = Find<RectTransform>("Ribbon");
            _ribbonLabel = Find<TMP_Text>("Ribbon/Label");
            _timerLabel = Find<TMP_Text>("Timer");
            _buyButton = Find<Button>("Buy");
            _priceLabel = Find<TMP_Text>("Buy/Label");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
