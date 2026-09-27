using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // One tape on the archive shelf: its number and title, and what it takes to surface while it is locked. A tape not yet
    // heard glows amber with a pulsing NEW chip; a heard one settles to the HUD's colour; a locked one is a padlock.
    public sealed class TapeRowView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _code;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Image _icon;
        [SerializeField] private Graphic _border;
        [SerializeField] private Graphic _play;
        [SerializeField] private RectTransform _newMark;
        [SerializeField] private Sprite _tapeSprite;
        [SerializeField] private Sprite _lockSprite;
        [SerializeField] private Color _newColor = new Color(1f, 0.71f, 0.28f, 1f);
        [SerializeField] private Color _heardColor = new Color(0.31f, 0.96f, 0.9f, 1f);
        [SerializeField] private Color _lockedColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color _textColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField, Range(0f, 1f)] private float _borderAlpha = 0.45f;
        [SerializeField] private float _markPulseScale = 1.12f;
        [SerializeField, Min(0.1f)] private float _markPulsePeriod = 0.9f;

        private TapeRowState? _state;

        public event Action Clicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetContent(string code, string title, string status, TapeRowState state)
        {
            _code.text = code;
            _title.text = title;
            _status.text = status;
            _status.gameObject.SetActive(!string.IsNullOrEmpty(status));
            if (_state == state)
                return;

            _state = state;
            var accent = state == TapeRowState.New ? _newColor : state == TapeRowState.Heard ? _heardColor : _lockedColor;
            _icon.sprite = state == TapeRowState.Locked ? _lockSprite : _tapeSprite;
            _icon.color = accent;
            _code.color = accent;
            _title.color = state == TapeRowState.Locked ? _lockedColor : _textColor;
            var border = accent;
            border.a = _borderAlpha;
            _border.color = border;
            _play.gameObject.SetActive(state != TapeRowState.Locked);
            _play.color = accent;
            SetMark(state == TapeRowState.New);
        }

        public void PlayDenied()
        {
            transform.DOKill(true);
            transform.DOShakePosition(0.35f, new Vector3(14f, 0f, 0f), 18, 0f).Ui(gameObject);
        }

        private void SetMark(bool visible)
        {
            _newMark.gameObject.SetActive(visible);
            _newMark.DOKill();
            _newMark.localScale = Vector3.one;
            if (visible)
                _newMark.DOScale(_markPulseScale, _markPulsePeriod * 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Ui(_newMark.gameObject);
        }

        // The pulse dies with the row when the screen closes; it starts again when the archive is opened next.
        private void OnEnable()
        {
            if (_state == TapeRowState.New)
                SetMark(true);
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
            _code = transform.Find("Code")?.GetComponent<TMP_Text>();
            _title = transform.Find("Title")?.GetComponent<TMP_Text>();
            _status = transform.Find("Status")?.GetComponent<TMP_Text>();
            _icon = transform.Find("Icon")?.GetComponent<Image>();
            _border = transform.Find("Border")?.GetComponent<Graphic>();
            _play = transform.Find("Play")?.GetComponent<Graphic>();
            _newMark = transform.Find("NewMark") as RectTransform;
        }
#endif
    }
}
