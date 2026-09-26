using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The agency's orders for today: three cards, the wallet, and when the next orders arrive. Claimed ectoplasm
    // visibly flies from the card into the wallet.
    public sealed class ContractsView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _balanceLabel;
        [SerializeField] private RectTransform _balanceIcon;
        [SerializeField] private TMP_Text _countdownLabel;
        [SerializeField] private ContractCardView[] _cards;
        [SerializeField] private RectTransform[] _flyers;
        [SerializeField, Min(0.05f)] private float _countDuration = 0.6f;
        [SerializeField, Min(0.05f)] private float _flyDuration = 0.55f;
        [SerializeField, Min(0f)] private float _flyStagger = 0.06f;

        private Action[] _claimHandlers;
        private Action[] _replaceHandlers;
        private int _shownBalance = -1;

        public event Action BackClicked;

        public event Action<int> ClaimClicked;

        public event Action<int> ReplaceClicked;

        public int CardCount => _cards.Length;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public ContractCardView Card(int index)
        {
            return _cards[index];
        }

        public void SetCountdown(string text)
        {
            _countdownLabel.text = text;
        }

        public void SetBalance(int amount)
        {
            if (_shownBalance < 0 || !isActiveAndEnabled)
            {
                ShowBalance(amount);
                return;
            }

            DOTween.To(() => _shownBalance, ShowBalance, amount, _countDuration).SetDelay(_flyDuration).SetEase(Ease.OutCubic)
                .Ui(gameObject);
        }

        public void PlayClaimed(int index)
        {
            var from = _cards[index].RewardIcon;
            for (var i = 0; i < _flyers.Length; i++)
            {
                var flyer = _flyers[i];
                var image = flyer.GetComponent<Image>();
                image.sprite = from.sprite;
                image.color = from.color;
                flyer.DOKill();
                flyer.gameObject.SetActive(true);
                flyer.position = from.rectTransform.position;
                flyer.localScale = Vector3.one;
                var delay = i * _flyStagger;
                flyer.DOMove(_balanceIcon.position, _flyDuration).SetDelay(delay).SetEase(Ease.InCubic).Ui(gameObject)
                    .OnComplete(() => flyer.gameObject.SetActive(false));
                flyer.DOScale(0.6f, _flyDuration).SetDelay(delay).SetEase(Ease.InQuad).Ui(gameObject);
            }

            _balanceIcon.DOKill(true);
            _balanceIcon.DOPunchScale(Vector3.one * 0.35f, _countDuration, 6).SetDelay(_flyDuration).Ui(gameObject);
        }

        private void ShowBalance(int amount)
        {
            _shownBalance = amount;
            _balanceLabel.text = amount.ToString();
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackClicked);
            _claimHandlers = new Action[_cards.Length];
            _replaceHandlers = new Action[_cards.Length];
            for (var i = 0; i < _cards.Length; i++)
            {
                var index = i;
                _claimHandlers[i] = () => ClaimClicked?.Invoke(index);
                _replaceHandlers[i] = () => ReplaceClicked?.Invoke(index);
                _cards[i].ClaimClicked += _claimHandlers[i];
                _cards[i].ReplaceClicked += _replaceHandlers[i];
            }

            foreach (var flyer in _flyers)
                flyer.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
            if (_claimHandlers == null)
                return;

            for (var i = 0; i < _cards.Length; i++)
            {
                _cards[i].ClaimClicked -= _claimHandlers[i];
                _cards[i].ReplaceClicked -= _replaceHandlers[i];
            }
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _backButton = transform.Find("BackButton")?.GetComponent<Button>();
            _balanceLabel = transform.Find("Balance/Amount")?.GetComponent<TMP_Text>();
            _balanceIcon = transform.Find("Balance/Icon") as RectTransform;
            _countdownLabel = transform.Find("Countdown")?.GetComponent<TMP_Text>();
            _cards = GetComponentsInChildren<ContractCardView>(true);
            var flyers = transform.Find("Flyers");
            if (flyers != null)
            {
                _flyers = new RectTransform[flyers.childCount];
                for (var i = 0; i < flyers.childCount; i++)
                    _flyers[i] = (RectTransform)flyers.GetChild(i);
            }
        }
#endif
    }
}
