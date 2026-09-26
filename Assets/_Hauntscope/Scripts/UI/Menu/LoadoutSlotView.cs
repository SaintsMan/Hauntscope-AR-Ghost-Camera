using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // A gear slot under START HUNT: lit when the item goes into the next hunt, dim when left behind, a "+" when empty.
    public sealed class LoadoutSlotView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Graphic _border;
        [SerializeField] private Graphic _fill;
        [SerializeField] private TMP_Text _countLabel;
        [SerializeField] private GameObject _plus;
        [SerializeField] private Color _offColor = new Color(0.49f, 0.55f, 0.6f, 0.55f);
        [SerializeField, Range(0f, 1f)] private float _emptyAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _armedFillAlpha = 0.18f;

        private Color _accent = Color.white;

        public event Action Clicked;

        public void SetGear(Sprite icon, Color accent)
        {
            _icon.sprite = icon;
            _accent = accent;
        }

        public void SetState(string count, bool empty, bool armed)
        {
            _countLabel.text = count;
            _countLabel.gameObject.SetActive(!empty);
            _plus.SetActive(empty);

            var iconColor = armed ? _accent : _offColor;
            if (empty)
                iconColor.a = _emptyAlpha;
            _icon.color = iconColor;
            _countLabel.color = armed ? _accent : _offColor;
            _border.color = armed ? _accent : _offColor;
            var fill = _accent;
            fill.a = armed ? _armedFillAlpha : 0f;
            _fill.color = fill;
        }

        public void PlayToggle(bool armed)
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * (armed ? 0.15f : -0.1f), 0.3f, 6).Ui(gameObject);
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
            _border = Find<Graphic>("Border");
            _fill = Find<Graphic>("Tint");
            _countLabel = Find<TMP_Text>("Count");
            var plus = transform.Find("Plus");
            _plus = plus != null ? plus.gameObject : null;
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
