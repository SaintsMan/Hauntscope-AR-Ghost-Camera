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
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _shiftButton;
        [SerializeField] private CanvasGroup _shiftGroup;
        [SerializeField] private TMP_Text _shiftCaption;
        [SerializeField, Range(0f, 1f)] private float _lockedAlpha = 0.45f;
        [SerializeField] private TMP_Text _ectoplasmLabel;
        [SerializeField] private TMP_Text _versionLabel;
        [SerializeField] private TMP_Text _timestampLabel;
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private TMP_Text _channelLabel;
        [SerializeField] private Graphic _statusDot;
        [SerializeField] private Color _statusColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _standbyDotColor = new Color(0.24f, 1f, 0.43f, 1f);
        [SerializeField] private Color _witchingHourColor = new Color(1f, 0.23f, 0.23f, 1f);
        [SerializeField] private RectTransform _ectoplasmIcon;
        [SerializeField, Min(0.05f)] private float _countDuration = 0.9f;
        [SerializeField] private Button _arModeButton;
        [SerializeField] private Button _virtualModeButton;
        [SerializeField] private RectTransform _modeThumb;
        [SerializeField] private Graphic[] _arModeGraphics;
        [SerializeField] private Graphic[] _virtualModeGraphics;
        [SerializeField] private TMP_Text _modeCaption;
        [SerializeField] private Color _modeOnColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _modeOffColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color _modeDisabledColor = new Color(0.49f, 0.55f, 0.6f, 0.35f);
        [SerializeField, Min(0.05f)] private float _thumbDuration = 0.28f;

        private bool _arAvailable = true;
        private bool _isVirtual;
        private bool _modeShown;

        private int _shownEctoplasm = -1;

        public event Action StartClicked;

        public event Action ShiftClicked;

        public event Action BestiaryClicked;

        public event Action SettingsClicked;

        public event Action ShopClicked;

        public event Action ArModeClicked;

        public event Action VirtualModeClicked;

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

        // The highlight slides under the chosen half of the switch, like a physical mode slider on a camcorder.
        public void SetMode(bool isVirtual)
        {
            var changed = _modeShown && isVirtual != _isVirtual;
            _isVirtual = isVirtual;
            _modeShown = true;
            var target = isVirtual ? ((RectTransform)_modeThumb.parent).rect.width * 0.5f : 0f;
            _modeThumb.DOKill();
            if (changed && isActiveAndEnabled)
            {
                _modeThumb.DOAnchorPosX(target, _thumbDuration).SetEase(Ease.OutBack, 1.4f).Ui(gameObject);
                _modeCaption.transform.DOKill();
                _modeCaption.transform.localScale = Vector3.one;
                _modeCaption.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 4).Ui(gameObject);
            }
            else
            {
                _modeThumb.anchoredPosition = new Vector2(target, _modeThumb.anchoredPosition.y);
            }

            RenderMode();
        }

        public void SetArAvailable(bool available)
        {
            _arAvailable = available;
            _arModeButton.interactable = available;
            RenderMode();
        }

        public void SetModeCaption(string text)
        {
            _modeCaption.text = text;
        }

        // STANDBY on channel 1 by day; channel 13 and a red light in the witching hour.
        public void SetStatus(string status, string channel, bool witchingHour)
        {
            _statusLabel.text = status;
            _statusLabel.color = witchingHour ? _witchingHourColor : _statusColor;
            _channelLabel.text = channel;
            _statusDot.color = witchingHour ? _witchingHourColor : _standbyDotColor;
        }

        // Locked, the shift button stays in view but dimmed, with how many hunts it takes to open it.
        public void SetShift(bool unlocked, string caption)
        {
            _shiftGroup.alpha = unlocked ? 1f : _lockedAlpha;
            _shiftCaption.text = caption;
            _shiftCaption.gameObject.SetActive(!string.IsNullOrEmpty(caption));
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
            _shiftButton.onClick.AddListener(OnShiftClicked);
            _bestiaryButton.onClick.AddListener(OnBestiaryClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _shopButton.onClick.AddListener(OnShopClicked);
            _arModeButton.onClick.AddListener(OnArModeClicked);
            _virtualModeButton.onClick.AddListener(OnVirtualModeClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _shiftButton.onClick.RemoveListener(OnShiftClicked);
            _bestiaryButton.onClick.RemoveListener(OnBestiaryClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _shopButton.onClick.RemoveListener(OnShopClicked);
            _arModeButton.onClick.RemoveListener(OnArModeClicked);
            _virtualModeButton.onClick.RemoveListener(OnVirtualModeClicked);
        }

        private void OnStartClicked()
        {
            StartClicked?.Invoke();
        }

        private void OnShiftClicked()
        {
            ShiftClicked?.Invoke();
        }

        private void OnBestiaryClicked()
        {
            BestiaryClicked?.Invoke();
        }

        private void OnSettingsClicked()
        {
            SettingsClicked?.Invoke();
        }

        private void OnShopClicked()
        {
            ShopClicked?.Invoke();
        }

        private void OnArModeClicked()
        {
            ArModeClicked?.Invoke();
        }

        private void OnVirtualModeClicked()
        {
            VirtualModeClicked?.Invoke();
        }

        private void RenderMode()
        {
            var arColor = !_arAvailable ? _modeDisabledColor : _isVirtual ? _modeOffColor : _modeOnColor;
            foreach (var graphic in _arModeGraphics)
                graphic.color = arColor;
            foreach (var graphic in _virtualModeGraphics)
                graphic.color = _isVirtual ? _modeOnColor : _modeOffColor;
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
            _shiftButton = Find<Button>("Buttons/ShiftButton");
            _shiftGroup = Find<CanvasGroup>("Buttons/ShiftButton");
            _shiftCaption = Find<TMP_Text>("Buttons/ShiftButton/Caption");
            _bestiaryButton = Find<Button>("Buttons/Row/BestiaryButton");
            _settingsButton = Find<Button>("Buttons/SettingsButton");
            _shopButton = Find<Button>("Buttons/Row/ShopButton");
            _ectoplasmLabel = Find<TMP_Text>("Ectoplasm/Amount");
            _versionLabel = Find<TMP_Text>("Version");
            _timestampLabel = Find<TMP_Text>("Timestamp");
            _ectoplasmIcon = Find<RectTransform>("Ectoplasm/Icon");
            _arModeButton = Find<Button>("Buttons/ModeSwitch/Ar");
            _virtualModeButton = Find<Button>("Buttons/ModeSwitch/Virtual");
            _modeThumb = Find<RectTransform>("Buttons/ModeSwitch/Thumb");
            _modeCaption = Find<TMP_Text>("Buttons/ModeCaption");
            _statusLabel = Find<TMP_Text>("TopBar/Label");
            _channelLabel = Find<TMP_Text>("TopBar/Channel");
            _statusDot = Find<Graphic>("TopBar/Dot");
            var ar = transform.Find("Buttons/ModeSwitch/Ar");
            if (ar != null)
                _arModeGraphics = new Graphic[] { Find<Graphic>("Buttons/ModeSwitch/Ar/Icon"), Find<Graphic>("Buttons/ModeSwitch/Ar/Label") };
            var room = transform.Find("Buttons/ModeSwitch/Virtual");
            if (room != null)
                _virtualModeGraphics = new Graphic[] { Find<Graphic>("Buttons/ModeSwitch/Virtual/Icon"), Find<Graphic>("Buttons/ModeSwitch/Virtual/Label") };
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
