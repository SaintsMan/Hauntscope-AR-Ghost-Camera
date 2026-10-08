using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class ShopView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _balanceLabel;
        [SerializeField] private RectTransform _balanceIcon;
        [SerializeField] private Button _lasersTab;
        [SerializeField] private Button _gearTab;
        [SerializeField] private Button _suppliesTab;
        [SerializeField] private Graphic[] _lasersTabGraphics;
        [SerializeField] private Graphic[] _gearTabGraphics;
        [SerializeField] private Graphic[] _suppliesTabGraphics;
        [SerializeField] private Graphic _lasersTabFill;
        [SerializeField] private Graphic _gearTabFill;
        [SerializeField] private Graphic _suppliesTabFill;
        [SerializeField] private RectTransform _lasersList;
        [SerializeField] private RectTransform _gearList;
        [SerializeField] private RectTransform _suppliesList;
        [SerializeField] private Button _topUpButton;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Color _tabOnColor = new Color(0.31f, 0.96f, 0.9f, 1f);
        [SerializeField] private Color _tabOffColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField, Range(0f, 1f)] private float _tabFillAlpha = 0.2f;
        [SerializeField, Min(0.05f)] private float _countDuration = 0.6f;
        [SerializeField] private Button _dropButton;
        [SerializeField] private CanvasGroup _dropGroup;
        [SerializeField] private TMP_Text _dropBody;
        [SerializeField] private TMP_Text _dropCounter;
        [SerializeField] private TMP_Text _dropButtonLabel;
        [SerializeField] private RectTransform _dropIcon;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.45f;

        private int _shownBalance = -1;

        public event Action BackClicked;

        public event Action<ShopTab> TabClicked;

        public event Action FieldDropClicked;
        public event Action TopUpClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetFieldDrop(string body, string counter, string buttonLabel, bool ready)
        {
            _dropBody.text = body;
            _dropCounter.text = counter;
            _dropButtonLabel.text = buttonLabel;
            _dropButton.interactable = ready;
            _dropGroup.alpha = ready ? 1f : _disabledAlpha;
        }

        public void PlayFieldDropClaimed()
        {
            _dropIcon.DOKill(true);
            _dropIcon.DOPunchScale(Vector3.one * 0.4f, 0.5f, 6).Ui(gameObject);
        }

        public ShopItemView AddItem(ShopTab tab)
        {
            return Instantiate(_itemPrefab, tab == ShopTab.Lasers ? _lasersList : _gearList);
        }

        public void SetTab(ShopTab tab)
        {
            var list = tab == ShopTab.Lasers ? _lasersList : tab == ShopTab.Gear ? _gearList : _suppliesList;
            _lasersList.gameObject.SetActive(tab == ShopTab.Lasers);
            _gearList.gameObject.SetActive(tab == ShopTab.Gear);
            _suppliesList.gameObject.SetActive(tab == ShopTab.Supplies);
            _scroll.content = list;
            _scroll.verticalNormalizedPosition = 1f;
            Paint(_lasersTabGraphics, _lasersTabFill, tab == ShopTab.Lasers);
            Paint(_gearTabGraphics, _gearTabFill, tab == ShopTab.Gear);
            Paint(_suppliesTabGraphics, _suppliesTabFill, tab == ShopTab.Supplies);
        }

        // The balance ticks down after a purchase, so the price is felt leaving the wallet.
        public void SetBalance(int amount)
        {
            if (_shownBalance < 0 || !isActiveAndEnabled)
            {
                ShowBalance(amount);
                return;
            }

            DOTween.To(() => _shownBalance, ShowBalance, amount, _countDuration).SetEase(Ease.OutCubic).Ui(gameObject);
            _balanceIcon.DOKill(true);
            _balanceIcon.DOPunchScale(Vector3.one * 0.3f, _countDuration, 6).Ui(gameObject);
        }

        private void ShowBalance(int amount)
        {
            _shownBalance = amount;
            _balanceLabel.text = amount.ToString();
        }

        private void Paint(Graphic[] graphics, Graphic fill, bool on)
        {
            foreach (var graphic in graphics)
                graphic.color = on ? _tabOnColor : _tabOffColor;
            var color = _tabOnColor;
            color.a = on ? _tabFillAlpha : 0f;
            fill.color = color;
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackClicked);
            _lasersTab.onClick.AddListener(OnLasersClicked);
            _gearTab.onClick.AddListener(OnGearClicked);
            _dropButton.onClick.AddListener(OnDropClicked);
            _suppliesTab.onClick.AddListener(OnSuppliesClicked);
            _topUpButton.onClick.AddListener(OnTopUpClicked);
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
            _lasersTab.onClick.RemoveListener(OnLasersClicked);
            _gearTab.onClick.RemoveListener(OnGearClicked);
            _dropButton.onClick.RemoveListener(OnDropClicked);
            _suppliesTab.onClick.RemoveListener(OnSuppliesClicked);
            _topUpButton.onClick.RemoveListener(OnTopUpClicked);
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
        }

        private void OnLasersClicked()
        {
            TabClicked?.Invoke(ShopTab.Lasers);
        }

        private void OnGearClicked()
        {
            TabClicked?.Invoke(ShopTab.Gear);
        }

        private void OnDropClicked()
        {
            FieldDropClicked?.Invoke();
        }

        private void OnSuppliesClicked()
        {
            TabClicked?.Invoke(ShopTab.Supplies);
        }

        private void OnTopUpClicked()
        {
            TopUpClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _backButton = Find<Button>("BackButton");
            _balanceLabel = Find<TMP_Text>("Balance/Amount");
            _balanceIcon = Find<RectTransform>("Balance/Icon");
            _lasersTab = Find<Button>("Tabs/Lasers");
            _gearTab = Find<Button>("Tabs/Gear");
            _suppliesTab = Find<Button>("Tabs/Supplies");
            _lasersTabGraphics = new Graphic[] { Find<Graphic>("Tabs/Lasers/Border"), Find<Graphic>("Tabs/Lasers/Label") };
            _gearTabGraphics = new Graphic[] { Find<Graphic>("Tabs/Gear/Border"), Find<Graphic>("Tabs/Gear/Label") };
            _lasersTabFill = Find<Graphic>("Tabs/Lasers/Fill");
            _gearTabFill = Find<Graphic>("Tabs/Gear/Fill");
            _suppliesTabGraphics = new Graphic[] { Find<Graphic>("Tabs/Supplies/Border"), Find<Graphic>("Tabs/Supplies/Label") };
            _suppliesTabFill = Find<Graphic>("Tabs/Supplies/Fill");
            _suppliesList = Find<RectTransform>("Scroll/Viewport/Supplies");
            _topUpButton = Find<Button>("Balance");
            _scroll = Find<ScrollRect>("Scroll");
            _lasersList = Find<RectTransform>("Scroll/Viewport/Lasers");
            _gearList = Find<RectTransform>("Scroll/Viewport/Gear");
            _dropButton = Find<Button>("FieldDrop/Watch");
            _dropGroup = Find<CanvasGroup>("FieldDrop/Watch");
            _dropBody = Find<TMP_Text>("FieldDrop/Body");
            _dropCounter = Find<TMP_Text>("FieldDrop/Counter");
            _dropButtonLabel = Find<TMP_Text>("FieldDrop/Watch/Row/Label");
            _dropIcon = Find<RectTransform>("FieldDrop/Icon");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
