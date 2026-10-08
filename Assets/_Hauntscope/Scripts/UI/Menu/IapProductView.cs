using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // One product sold for money: an ectoplasm tile in the grid or a kit card in the list. The ribbon calls out a bonus
    // or the best deal; the button carries the store's own price.
    public sealed class IapProductView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Graphic _iconGlow;
        [SerializeField] private Graphic _border;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _detailLabel;
        [SerializeField] private GameObject _ribbon;
        [SerializeField] private Graphic _ribbonFill;
        [SerializeField] private TMP_Text _ribbonLabel;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Graphic _buyBorder;
        [SerializeField] private Graphic _buyFill;
        [SerializeField] private TMP_Text _priceLabel;
        [SerializeField] private Color _textColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _dimColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField, Range(0f, 1f)] private float _glowAlpha = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _borderAlpha = 0.75f;
        [SerializeField, Range(0f, 1f)] private float _buyFillAlpha = 0.22f;

        private Color _accent = Color.white;

        public event Action BuyClicked;

        public void SetContent(Sprite icon, Color accent, string itemName, string detail)
        {
            _accent = accent;
            _icon.sprite = icon;
            _icon.color = accent;
            _iconGlow.color = WithAlpha(accent, _glowAlpha);
            _border.color = WithAlpha(accent, _borderAlpha);
            _nameLabel.text = itemName;
            _detailLabel.text = detail;
        }

        // Empty hides the ribbon.
        public void SetRibbon(string text, Color color)
        {
            _ribbon.SetActive(!string.IsNullOrEmpty(text));
            _ribbonLabel.text = text;
            _ribbonFill.color = color;
        }

        public void SetPrice(string label, bool available)
        {
            _priceLabel.text = label;
            _priceLabel.color = available ? _textColor : _dimColor;
            _buyButton.interactable = available;
            _buyBorder.color = available ? _accent : WithAlpha(_dimColor, _borderAlpha);
            _buyFill.color = WithAlpha(_accent, available ? _buyFillAlpha : 0f);
        }

        public void PlayPurchased()
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.08f, 0.45f, 6).Ui(gameObject);
            _iconGlow.DOKill(true);
            _iconGlow.DOFade(1f, 0.14f).SetLoops(2, LoopType.Yoyo).Ui(gameObject);
            _icon.transform.DOKill(true);
            _icon.transform.DOPunchRotation(new Vector3(0f, 0f, 14f), 0.55f, 8).Ui(gameObject);
        }

        public void PlayDenied()
        {
            _buyButton.transform.DOKill(true);
            _buyButton.transform.DOShakePosition(0.35f, new Vector3(14f, 0f, 0f), 18, 0f).Ui(gameObject);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
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
            _icon = Find<Image>("Icon");
            _iconGlow = Find<Graphic>("IconGlow");
            _border = Find<Graphic>("Border");
            _nameLabel = Find<TMP_Text>("Name");
            _detailLabel = Find<TMP_Text>("Detail");
            var ribbon = transform.Find("Ribbon");
            _ribbon = ribbon != null ? ribbon.gameObject : null;
            _ribbonFill = Find<Graphic>("Ribbon/Fill");
            _ribbonLabel = Find<TMP_Text>("Ribbon/Label");
            _buyButton = Find<Button>("Buy");
            _buyBorder = Find<Graphic>("Buy/Border");
            _buyFill = Find<Graphic>("Buy/Tint");
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
