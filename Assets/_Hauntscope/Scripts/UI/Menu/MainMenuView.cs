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
        [SerializeField] private TMP_Text _ectoplasmLabel;
        [SerializeField] private TMP_Text _versionLabel;
        [SerializeField] private TMP_Text _timestampLabel;
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

        public event Action BestiaryClicked;

        public event Action SettingsClicked;

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
            _bestiaryButton.onClick.AddListener(OnBestiaryClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _arModeButton.onClick.AddListener(OnArModeClicked);
            _virtualModeButton.onClick.AddListener(OnVirtualModeClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _bestiaryButton.onClick.RemoveListener(OnBestiaryClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _arModeButton.onClick.RemoveListener(OnArModeClicked);
            _virtualModeButton.onClick.RemoveListener(OnVirtualModeClicked);
        }

        private void OnStartClicked()
        {
            StartClicked?.Invoke();
        }

        private void OnBestiaryClicked()
        {
            BestiaryClicked?.Invoke();
        }

        private void OnSettingsClicked()
        {
            SettingsClicked?.Invoke();
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
            _bestiaryButton = Find<Button>("Buttons/BestiaryButton");
            _settingsButton = Find<Button>("Buttons/SettingsButton");
            _ectoplasmLabel = Find<TMP_Text>("Ectoplasm/Amount");
            _versionLabel = Find<TMP_Text>("Version");
            _timestampLabel = Find<TMP_Text>("Timestamp");
            _ectoplasmIcon = Find<RectTransform>("Ectoplasm/Icon");
            _arModeButton = Find<Button>("Buttons/ModeSwitch/Ar");
            _virtualModeButton = Find<Button>("Buttons/ModeSwitch/Virtual");
            _modeThumb = Find<RectTransform>("Buttons/ModeSwitch/Thumb");
            _modeCaption = Find<TMP_Text>("Buttons/ModeCaption");
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
