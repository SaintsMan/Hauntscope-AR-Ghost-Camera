using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Research;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class BestiaryView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _countLabel;
        [SerializeField] private RectTransform _cardsRoot;
        [SerializeField] private BestiaryCardView _cardPrefab;
        [SerializeField] private GameObject _details;
        [SerializeField] private Image _detailsIcon;
        [SerializeField] private Image _detailsBorder;
        [SerializeField] private Graphic _detailsGlow;
        [SerializeField] private TMP_Text _detailsName;
        [SerializeField] private TMP_Text _detailsCase;
        [SerializeField] private TMP_Text _detailsStamp;
        [SerializeField] private Graphic _detailsStampFrame;
        [SerializeField] private Image[] _threatLeds;
        [SerializeField] private TMP_Text _detailsLore;
        [SerializeField] private ScrollRect _detailsScroll;
        [SerializeField] private Button _detailsCloseButton;
        [SerializeField] private Typewriter _detailsLoreTypewriter;
        [SerializeField] private Color _silhouetteColor = new Color(0.01f, 0.015f, 0.025f, 0.95f);
        [SerializeField] private Color _unknownColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color _sightedStampColor = new Color(1f, 0.71f, 0.28f, 1f);
        [SerializeField] private Color _capturedStampColor = new Color(1f, 0.23f, 0.23f, 1f);
        [SerializeField] private Color _declassifiedStampColor = new Color(1f, 0.82f, 0.4f, 1f);
        [SerializeField] private Color[] _threatColors;
        [SerializeField, Range(0f, 1f)] private float _ledUnlitAlpha = 0.15f;
        [SerializeField, Range(0f, 1f)] private float _glowAlpha = 0.35f;
        [SerializeField] private Color _lockedTextColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color _redactionColor = new Color(0.16f, 0.2f, 0.25f, 1f);
        [SerializeField] private Color _bodyTextColor = new Color(0.9f, 0.93f, 0.95f, 1f);

        private DossierFormatter _formatter;

        public event Action BackClicked;

        public event Action DetailsCloseClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetCount(string text)
        {
            _countLabel.text = text;
        }

        public BestiaryCardView AddCard()
        {
            return Instantiate(_cardPrefab, _cardsRoot);
        }

        public void ShowDetails(DossierHeader header, IReadOnlyList<DossierSection> sections, bool retype)
        {
            var known = header.Level >= ResearchLevel.Captured;
            var seen = header.Level >= ResearchLevel.Sighted;
            var accent = seen ? header.Accent : _unknownColor;

            _detailsIcon.sprite = header.Icon;
            _detailsIcon.color = known ? Color.white : _silhouetteColor;
            _detailsBorder.color = accent;
            var glow = accent;
            glow.a = seen ? _glowAlpha : _glowAlpha * 0.4f;
            _detailsGlow.color = glow;
            _detailsName.text = header.Name;
            _detailsName.color = accent;
            _detailsCase.text = header.CaseLine;
            _detailsStamp.text = header.Stamp;
            var stamp = StampColor(header.Level);
            _detailsStamp.color = stamp;
            _detailsStampFrame.color = stamp;
            SetThreat(header.Threat);

            _formatter ??= new DossierFormatter(_lockedTextColor, _redactionColor, _bodyTextColor);
            _detailsLore.text = _formatter.Format(sections, accent);
            _details.SetActive(true);
            if (!retype)
                return;

            _detailsScroll.verticalNormalizedPosition = 1f;
            _detailsLoreTypewriter.Play();
        }

        public void HideDetails()
        {
            _details.SetActive(false);
        }

        private void SetThreat(int threat)
        {
            for (var i = 0; i < _threatLeds.Length; i++)
            {
                var color = _threatColors[Mathf.Min(threat, _threatColors.Length) - 1];
                if (i >= threat)
                    color.a *= _ledUnlitAlpha;
                _threatLeds[i].color = color;
            }
        }

        private Color StampColor(ResearchLevel level)
        {
            switch (level)
            {
                case ResearchLevel.Sighted:
                    return _sightedStampColor;
                case ResearchLevel.Captured:
                    return _capturedStampColor;
                case ResearchLevel.Declassified:
                    return _declassifiedStampColor;
                default:
                    return _unknownColor;
            }
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackClicked);
            _detailsCloseButton.onClick.AddListener(OnDetailsCloseClicked);
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
            _detailsCloseButton.onClick.RemoveListener(OnDetailsCloseClicked);
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
        }

        private void OnDetailsCloseClicked()
        {
            DetailsCloseClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _backButton = Find<Button>("BackButton");
            _countLabel = Find<TMP_Text>("Count");
            var details = transform.Find("Details");
            _details = details != null ? details.gameObject : null;
            _detailsIcon = Find<Image>("Details/Card/Icon");
            _detailsBorder = Find<Image>("Details/Card/Border");
            _detailsGlow = Find<Graphic>("Details/Card/Glow");
            _detailsName = Find<TMP_Text>("Details/Card/Name");
            _detailsCase = Find<TMP_Text>("Details/Card/Case");
            _detailsStamp = Find<TMP_Text>("Details/Card/Stamp/Label");
            _detailsStampFrame = Find<Graphic>("Details/Card/Stamp");
            var leds = transform.Find("Details/Card/Threat/Leds");
            if (leds != null)
                _threatLeds = leds.GetComponentsInChildren<Image>(true);
            _detailsScroll = Find<ScrollRect>("Details/Card/Body");
            _detailsLore = Find<TMP_Text>("Details/Card/Body/Viewport/Lore");
            _detailsLoreTypewriter = Find<Typewriter>("Details/Card/Body/Viewport/Lore");
            _detailsCloseButton = Find<Button>("Details/Card/CloseButton");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
