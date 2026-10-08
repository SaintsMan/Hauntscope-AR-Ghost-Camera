using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The menu's ways into the paid supplies: tapping the ectoplasm balance, and a ribbon on the shop tile while the
    // rookie kit offer runs.
    public sealed class MenuStoreLinkView : MonoBehaviour
    {
        [SerializeField] private Button _balanceButton;
        [SerializeField] private RectTransform _offerBadge;
        [SerializeField] private TMP_Text _offerLabel;
        [SerializeField] private float _badgePulse = 1.12f;
        [SerializeField, Min(0.1f)] private float _badgePeriod = 0.9f;

        public event Action BalanceClicked;

        public void SetOffer(string label)
        {
            var visible = !string.IsNullOrEmpty(label);
            _offerLabel.text = label;
            if (_offerBadge.gameObject.activeSelf == visible)
                return;

            _offerBadge.gameObject.SetActive(visible);
            _offerBadge.DOKill();
            _offerBadge.localScale = Vector3.one;
            if (visible)
                _offerBadge.DOScale(_badgePulse, _badgePeriod * 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Ui(gameObject);
        }

        private void Awake()
        {
            _balanceButton.onClick.AddListener(OnBalanceClicked);
        }

        private void OnDestroy()
        {
            _balanceButton.onClick.RemoveListener(OnBalanceClicked);
        }

        private void OnBalanceClicked()
        {
            BalanceClicked?.Invoke();
        }
    }
}
