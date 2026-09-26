using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    public sealed class ResultView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _ghostNameLabel;
        [SerializeField] private TMP_Text _rewardLabel;
        [SerializeField] private TMP_Text _timeLabel;
        [SerializeField] private Button _huntAgainButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Color _capturedColor;
        [SerializeField] private Color _escapedColor;
        [SerializeField] private Material _capturedTitleMaterial;
        [SerializeField] private Material _escapedTitleMaterial;
        [SerializeField] private Image _ghostIcon;
        [SerializeField] private Graphic _ghostGlow;
        [SerializeField] private Color _escapedIconColor = new Color(0.25f, 0.28f, 0.32f, 0.9f);
        [SerializeField, Range(0f, 1f)] private float _glowAlpha = 0.35f;
        [SerializeField, FormerlySerializedAs("_reasonLabel")] private TMP_Text _tipLabel;
        [SerializeField] private GameObject _newEntryBadge;
        [SerializeField] private TMP_Text _badgeLabel;
        [SerializeField] private Button _doubleButton;
        [SerializeField] private CanvasGroup _doubleGroup;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.4f;
        [SerializeField] private GameObject _reward;
        [SerializeField] private RectTransform _rewardIcon;
        [SerializeField, Min(0f)] private float _countDelay = 0.45f;
        [SerializeField, Min(0.05f)] private float _countDuration = 0.9f;
        [SerializeField] private TMP_Text _breakdownLabel;

        private Func<int, string> _rewardFormat;
        private int _shownReward;
        private int _rewardTarget = -1;

        public event Action HuntAgainClicked;

        public event Action MenuClicked;

        public event Action DoubleClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetTitle(string text, bool captured)
        {
            _titleLabel.text = text;
            _titleLabel.color = captured ? _capturedColor : _escapedColor;
            _titleLabel.fontSharedMaterial = captured ? _capturedTitleMaterial : _escapedTitleMaterial;
        }

        // A caught ghost floats in full colour inside its glow; one that got away is only a dark silhouette.
        public void SetGhost(Sprite icon, Color accent, bool captured)
        {
            _ghostIcon.sprite = icon;
            _ghostIcon.color = captured ? Color.white : _escapedIconColor;
            var glow = captured ? accent : _escapedColor;
            glow.a = _glowAlpha;
            _ghostGlow.color = glow;
        }

        public void SetTip(string text)
        {
            _tipLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _tipLabel.text = text;
        }

        // A stamp for a Bestiary milestone of this catch: a new entry, or a file declassified. Empty hides it.
        public void SetBadge(string text)
        {
            _newEntryBadge.SetActive(!string.IsNullOrEmpty(text));
            _badgeLabel.text = text;
        }

        public void SetGhostName(string text)
        {
            _ghostNameLabel.text = text;
        }

        // The reward counts up from zero once the card has opened, like the ectoplasm balance in the menu.
        // The presenter owns the wording, so the label is rebuilt through its format for every shown number.
        public void SetReward(int amount, Func<int, string> format)
        {
            _reward.SetActive(amount > 0);
            _rewardFormat = format;
            if (amount == _rewardTarget)
            {
                ShowReward(_shownReward);
                return;
            }

            // The first reward counts up from zero; a doubled one counts on from what is already shown.
            var from = _rewardTarget > 0 ? _shownReward : 0;
            _rewardTarget = amount;
            if (amount <= 0)
                return;

            ShowReward(from);
            DOTween.To(() => _shownReward, ShowReward, amount, _countDuration)
                .SetDelay(_countDelay).SetEase(Ease.OutCubic).Ui(gameObject);
            _rewardIcon.localScale = Vector3.one;
            _rewardIcon.DOPunchScale(Vector3.one * 0.35f, _countDuration, 6).SetDelay(_countDelay).Ui(gameObject);
        }

        // Where the reward came from: the catch, the research bonus, pickups from the room.
        public void SetBreakdown(string text)
        {
            _breakdownLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _breakdownLabel.text = text;
        }

        public void SetDouble(bool visible, bool ready)
        {
            _doubleButton.gameObject.SetActive(visible);
            _doubleButton.interactable = ready;
            _doubleGroup.alpha = ready ? 1f : _disabledAlpha;
        }

        public void SetTime(string text)
        {
            _timeLabel.text = text;
        }

        private void Awake()
        {
            _huntAgainButton.onClick.AddListener(OnHuntAgainClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
            _doubleButton.onClick.AddListener(OnDoubleClicked);
        }

        // A closed card forgets its reward, so the next catch counts up again even when it pays the same.
        private void OnDisable()
        {
            _rewardTarget = -1;
        }

        private void OnDestroy()
        {
            _huntAgainButton.onClick.RemoveListener(OnHuntAgainClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
            _doubleButton.onClick.RemoveListener(OnDoubleClicked);
        }

        private void OnHuntAgainClicked()
        {
            HuntAgainClicked?.Invoke();
        }

        private void OnMenuClicked()
        {
            MenuClicked?.Invoke();
        }

        private void OnDoubleClicked()
        {
            DoubleClicked?.Invoke();
        }

        private void ShowReward(int amount)
        {
            _shownReward = amount;
            _rewardLabel.text = _rewardFormat(amount);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _titleLabel = Find<TMP_Text>("Card/Title");
            _ghostNameLabel = Find<TMP_Text>("Card/GhostName");
            var reward = transform.Find("Card/Reward");
            _reward = reward != null ? reward.gameObject : null;
            _rewardLabel = Find<TMP_Text>("Card/Reward/Amount");
            _rewardIcon = Find<RectTransform>("Card/Reward/Icon");
            _tipLabel = Find<TMP_Text>("Card/Tip");
            _timeLabel = Find<TMP_Text>("Card/Time");
            _breakdownLabel = Find<TMP_Text>("Card/Breakdown");
            var badge = transform.Find("Card/NewEntry");
            _newEntryBadge = badge != null ? badge.gameObject : null;
            _badgeLabel = Find<TMP_Text>("Card/NewEntry/Label");
            _doubleButton = Find<Button>("Card/DoubleButton");
            _doubleGroup = Find<CanvasGroup>("Card/DoubleButton");
            _huntAgainButton = Find<Button>("Card/HuntAgainButton");
            _menuButton = Find<Button>("Card/MenuButton");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
