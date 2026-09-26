using System;
using DG.Tweening;
using Hauntscope.Gameplay.Contracts;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // One agency order: number and tier, the condition, a progress bar, the pay, and CLAIM / REPLACE. A done order
    // gets a rubber stamp; a paid one fades back.
    public sealed class ContractCardView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _order;
        [SerializeField] private TMP_Text _tier;
        [SerializeField] private Graphic _tierFrame;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private RectTransform _progressFill;
        [SerializeField] private Graphic _progressFillGraphic;
        [SerializeField] private TMP_Text _progress;
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TMP_Text _reward;
        [SerializeField] private Button _claimButton;
        [SerializeField] private Button _replaceButton;
        [SerializeField] private CanvasGroup _replaceGroup;
        [SerializeField] private TMP_Text _replaceLabel;
        [SerializeField] private GameObject _replaceAdIcon;
        [SerializeField] private RectTransform _stamp;
        [SerializeField] private TMP_Text _stampLabel;
        [SerializeField] private Graphic _flash;
        [SerializeField] private Sprite _ectoplasmIcon;
        [SerializeField] private Color _ectoplasmColor = new Color(0.24f, 1f, 0.43f, 1f);
        [SerializeField] private Color[] _tierColors =
        {
            new Color(0.24f, 1f, 0.43f, 1f),
            new Color(1f, 0.71f, 0.28f, 1f),
            new Color(1f, 0.23f, 0.23f, 1f)
        };
        [SerializeField, Range(0f, 1f)] private float _paidAlpha = 0.55f;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.45f;
        [SerializeField, Min(0.05f)] private float _stampDuration = 0.35f;
        [SerializeField] private float _stampAngle = -10f;

        private bool _isStamped;

        public event Action ClaimClicked;

        public event Action ReplaceClicked;

        // Where a claimed reward flies from, and what it looks like on the way.
        public Image RewardIcon => _rewardIcon;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetHeader(string order, string tierLabel, ContractTier tier)
        {
            var color = _tierColors[Mathf.Clamp((int)tier, 0, _tierColors.Length - 1)];
            _order.text = order;
            _tier.text = tierLabel;
            _tier.color = color;
            _tierFrame.color = color;
            _progressFillGraphic.color = color;
        }

        public void SetDescription(string text)
        {
            _description.text = text;
        }

        public void SetProgress(float normalized, string label)
        {
            _progressFill.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
            _progress.text = label;
        }

        // No gear icon means the order pays ectoplasm.
        public void SetReward(Sprite gearIcon, string label)
        {
            _rewardIcon.sprite = gearIcon != null ? gearIcon : _ectoplasmIcon;
            _rewardIcon.color = gearIcon != null ? Color.white : _ectoplasmColor;
            _reward.text = label;
        }

        public void SetClaim(bool visible)
        {
            _claimButton.gameObject.SetActive(visible);
        }

        public void SetReplace(bool visible, bool interactable, bool viaAd, string label)
        {
            _replaceButton.gameObject.SetActive(visible);
            _replaceButton.interactable = interactable;
            _replaceGroup.alpha = interactable ? 1f : _disabledAlpha;
            _replaceAdIcon.SetActive(viaAd);
            _replaceLabel.text = label;
        }

        // The stamp lands the first time the order shows as done; after that it simply stays.
        public void SetStamp(bool visible, string label, bool paid, bool animate)
        {
            _stampLabel.text = label;
            _group.alpha = paid ? _paidAlpha : 1f;
            _stamp.gameObject.SetActive(visible);
            if (!visible)
            {
                _isStamped = false;
                return;
            }

            if (_isStamped || !animate || !isActiveAndEnabled)
            {
                _isStamped = true;
                _stamp.localScale = Vector3.one;
                _stamp.localEulerAngles = new Vector3(0f, 0f, _stampAngle);
                return;
            }

            _isStamped = true;
            _stamp.DOKill();
            _stamp.localScale = Vector3.one * 2.2f;
            _stamp.localEulerAngles = new Vector3(0f, 0f, _stampAngle - 18f);
            _stamp.DOScale(1f, _stampDuration).SetEase(Ease.OutBack).Ui(gameObject);
            _stamp.DOLocalRotate(new Vector3(0f, 0f, _stampAngle), _stampDuration).SetEase(Ease.OutQuad).Ui(gameObject);
        }

        // A new order is typed over the old one: a quick white flash across the card.
        public void PlayReplaced()
        {
            _flash.DOKill();
            var color = _flash.color;
            color.a = 0.5f;
            _flash.color = color;
            _flash.DOFade(0f, 0.35f).SetEase(Ease.OutQuad).Ui(gameObject);
            transform.DOKill(true);
            transform.DOPunchPosition(new Vector3(14f, 0f, 0f), 0.3f, 12).Ui(gameObject);
        }

        private void Awake()
        {
            _claimButton.onClick.AddListener(OnClaimClicked);
            _replaceButton.onClick.AddListener(OnReplaceClicked);
        }

        private void OnDestroy()
        {
            _claimButton.onClick.RemoveListener(OnClaimClicked);
            _replaceButton.onClick.RemoveListener(OnReplaceClicked);
        }

        private void OnClaimClicked()
        {
            ClaimClicked?.Invoke();
        }

        private void OnReplaceClicked()
        {
            ReplaceClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _group = GetComponent<CanvasGroup>();
            _order = Find<TMP_Text>("Order");
            _tier = Find<TMP_Text>("Tier/Label");
            _tierFrame = Find<Graphic>("Tier/Border");
            _description = Find<TMP_Text>("Description");
            _progressFill = Find<RectTransform>("Progress/Fill");
            _progressFillGraphic = Find<Graphic>("Progress/Fill");
            _progress = Find<TMP_Text>("Progress/Label");
            _rewardIcon = Find<Image>("Reward/Icon");
            _reward = Find<TMP_Text>("Reward/Label");
            _claimButton = Find<Button>("ClaimButton");
            _replaceButton = Find<Button>("ReplaceButton");
            _replaceGroup = Find<CanvasGroup>("ReplaceButton");
            _replaceLabel = Find<TMP_Text>("ReplaceButton/Row/Label");
            var adIcon = transform.Find("ReplaceButton/Row/AdIcon");
            _replaceAdIcon = adIcon != null ? adIcon.gameObject : null;
            _stamp = Find<RectTransform>("Stamp");
            _stampLabel = Find<TMP_Text>("Stamp/Label");
            _flash = Find<Graphic>("Flash");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
