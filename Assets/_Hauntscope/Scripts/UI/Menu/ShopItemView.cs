using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // One card of the supply depot: a laser (with LED ratings) or a gear stack. The button's look follows its state.
    public sealed class ShopItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Graphic _iconGlow;
        [SerializeField] private Graphic _border;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _descriptionLabel;
        [SerializeField] private TMP_Text _countLabel;
        [SerializeField] private GameObject _stats;
        [SerializeField] private Image[] _powerLeds;
        [SerializeField] private Image[] _holdLeds;
        [SerializeField] private Image[] _economyLeds;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Graphic _actionBorder;
        [SerializeField] private Graphic _actionFill;
        [SerializeField] private TMP_Text _actionLabel;
        [SerializeField] private GameObject _priceIcon;
        [SerializeField] private Color _textColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _dimColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color _idleBorderColor = new Color(0.49f, 0.55f, 0.6f, 0.6f);
        [SerializeField, Range(0f, 1f)] private float _ledUnlitAlpha = 0.15f;
        [SerializeField, Range(0f, 1f)] private float _equippedFillAlpha = 0.22f;
        [SerializeField, Range(0f, 1f)] private float _glowAlpha = 0.3f;
        [SerializeField, Min(0f)] private float _descriptionHeight = 100f;
        [SerializeField, Min(0f)] private float _descriptionTallHeight = 170f;

        private Color _accent = Color.white;

        public event Action ActionClicked;

        public void SetContent(Sprite icon, Color accent, string itemName, string description)
        {
            _accent = accent;
            _icon.sprite = icon;
            _icon.color = accent;
            var glow = accent;
            glow.a = _glowAlpha;
            _iconGlow.color = glow;
            _nameLabel.text = itemName;
            _descriptionLabel.text = description;
        }

        public void SetStats(int power, int hold, int economy)
        {
            _stats.SetActive(true);
            SetDescriptionHeight(_descriptionHeight);
            SetLeds(_powerLeds, power);
            SetLeds(_holdLeds, hold);
            SetLeds(_economyLeds, economy);
        }

        // Gear has no ratings, so its description takes the space the LED rows would use.
        public void HideStats()
        {
            _stats.SetActive(false);
            SetDescriptionHeight(_descriptionTallHeight);
        }

        public void SetCount(string text)
        {
            _countLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _countLabel.text = text;
        }

        public void SetAction(string label, ShopItemState state)
        {
            _actionLabel.text = label;
            _priceIcon.SetActive(state == ShopItemState.Buy || state == ShopItemState.CantAfford);

            var active = state == ShopItemState.Buy || state == ShopItemState.Equip;
            _actionLabel.color = state == ShopItemState.Equipped ? _accent : active ? _textColor : _dimColor;
            _actionBorder.color = state == ShopItemState.Buy || state == ShopItemState.Equipped ? _accent : active ? _textColor : _idleBorderColor;
            var fill = _accent;
            fill.a = state == ShopItemState.Equipped ? _equippedFillAlpha : 0f;
            _actionFill.color = fill;
            _border.color = state == ShopItemState.Equipped ? _accent : _idleBorderColor;
        }

        public void PlayPurchased()
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.06f, 0.4f, 6).Ui(gameObject);
            _iconGlow.DOKill(true);
            _iconGlow.DOFade(1f, 0.12f).SetLoops(2, LoopType.Yoyo).Ui(gameObject);
            _icon.transform.DOKill(true);
            _icon.transform.DOPunchRotation(new Vector3(0f, 0f, 12f), 0.5f, 8).Ui(gameObject);
        }

        public void PlayDenied()
        {
            _actionButton.transform.DOKill(true);
            _actionButton.transform.DOShakePosition(0.35f, new Vector3(14f, 0f, 0f), 18, 0f).Ui(gameObject);
        }

        private void SetDescriptionHeight(float height)
        {
            var rect = _descriptionLabel.rectTransform;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        }

        private void SetLeds(Image[] leds, int lit)
        {
            for (var i = 0; i < leds.Length; i++)
            {
                var color = _accent;
                if (i >= lit)
                    color.a *= _ledUnlitAlpha;
                leds[i].color = color;
            }
        }

        private void Awake()
        {
            _actionButton.onClick.AddListener(OnActionClicked);
        }

        private void OnDestroy()
        {
            _actionButton.onClick.RemoveListener(OnActionClicked);
        }

        private void OnActionClicked()
        {
            ActionClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _icon = Find<Image>("Icon");
            _iconGlow = Find<Graphic>("IconGlow");
            _border = Find<Graphic>("Border");
            _nameLabel = Find<TMP_Text>("Name");
            _descriptionLabel = Find<TMP_Text>("Description");
            _countLabel = Find<TMP_Text>("Count");
            var stats = transform.Find("Stats");
            _stats = stats != null ? stats.gameObject : null;
            _powerLeds = Leds("Stats/Power/Leds");
            _holdLeds = Leds("Stats/Hold/Leds");
            _economyLeds = Leds("Stats/Economy/Leds");
            _actionButton = Find<Button>("Action");
            _actionBorder = Find<Graphic>("Action/Border");
            _actionFill = Find<Graphic>("Action/Tint");
            _actionLabel = Find<TMP_Text>("Action/Row/Label");
            var price = transform.Find("Action/Row/Price");
            _priceIcon = price != null ? price.gameObject : null;
        }

        private Image[] Leds(string path)
        {
            var root = transform.Find(path);
            return root != null ? root.GetComponentsInChildren<Image>(true) : Array.Empty<Image>();
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
