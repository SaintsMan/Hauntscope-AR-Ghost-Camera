using System;
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
        [SerializeField] private TMP_Text _detailsName;
        [SerializeField] private TMP_Text _detailsLore;
        [SerializeField] private Button _detailsCloseButton;

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

        public void ShowDetails(Sprite icon, Color accent, string ghostName, string lore)
        {
            _detailsIcon.sprite = icon;
            _detailsBorder.color = accent;
            _detailsName.text = ghostName;
            _detailsName.color = accent;
            _detailsLore.text = lore;
            _details.SetActive(true);
        }

        public void HideDetails()
        {
            _details.SetActive(false);
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
    }
}
