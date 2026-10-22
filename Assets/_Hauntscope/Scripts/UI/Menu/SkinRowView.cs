using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // One skin on the event's shelf: what it repaints, its name and what tapping it does now (buy, wear, take off).
    public sealed class SkinRowView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _slot;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Image _icon;
        [SerializeField] private Graphic _price;
        [SerializeField] private ButtonJuice _juice;
        [SerializeField] private Color _buyColor = new Color(1f, 0.71f, 0.28f, 1f);
        [SerializeField] private Color _wornColor = new Color(0.24f, 1f, 0.43f, 1f);
        [SerializeField] private Color _ownedColor = new Color(0.31f, 0.96f, 0.9f, 1f);
        [SerializeField] private Color _lockedColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color _textColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField, Range(0f, 1f)] private float _borderAlpha = 0.5f;

        public event Action Clicked;

        public void SetContent(string slot, string name, string status, Sprite icon, SkinRowState state)
        {
            _slot.text = slot;
            _name.text = name;
            _status.text = status;
            _icon.sprite = icon;
            var accent = state == SkinRowState.Worn ? _wornColor : state == SkinRowState.Owned ? _ownedColor
                : state == SkinRowState.Locked ? _lockedColor : _buyColor;
            _icon.color = state == SkinRowState.Locked ? _lockedColor : Color.white;
            _slot.color = accent;
            _status.color = accent;
            _name.color = state == SkinRowState.Locked ? _lockedColor : _textColor;
            _price.gameObject.SetActive(state == SkinRowState.Buyable);
            _price.color = Color.white;
            var border = accent;
            border.a = _borderAlpha;
            _juice.SetFrameColor(border);
        }

        public void PlayDenied()
        {
            transform.DOKill(true);
            transform.DOShakePosition(0.35f, new Vector3(14f, 0f, 0f), 18, 0f).Ui(gameObject);
        }

        public void PlayBought()
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.06f, 0.5f, 6).Ui(gameObject);
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
            _slot = transform.Find("Text/Code")?.GetComponent<TMP_Text>();
            _name = transform.Find("Text/Title")?.GetComponent<TMP_Text>();
            _status = transform.Find("Text/Status")?.GetComponent<TMP_Text>();
            _icon = transform.Find("Icon")?.GetComponent<Image>();
            _price = transform.Find("Price")?.GetComponent<Graphic>();
            _juice = GetComponent<ButtonJuice>();
        }
#endif
    }
}
