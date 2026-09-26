using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class ShopPresenter : IStartable, IDisposable
    {
        private const string PriceKey = "shop.price";
        private const string EquipKey = "shop.equip";
        private const string EquippedKey = "shop.equipped";
        private const string MaxKey = "shop.max";
        private const string OwnedKey = "shop.owned";

        private readonly ShopView _view;
        private readonly MenuNavigation _navigation;
        private readonly StoreConfig _store;
        private readonly Shop _shop;
        private readonly PlayerInventory _inventory;
        private readonly PlayerProgress _progress;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly List<ShopItemView> _laserCards = new List<ShopItemView>();
        private readonly List<ShopItemView> _gearCards = new List<ShopItemView>();
        private readonly List<GearData> _gear = new List<GearData>();
        private readonly List<Action> _handlers = new List<Action>();
        private readonly List<ShopItemView> _handled = new List<ShopItemView>();

        public ShopPresenter(
            ShopView view,
            MenuNavigation navigation,
            StoreConfig store,
            Shop shop,
            PlayerInventory inventory,
            PlayerProgress progress,
            ILocalizationService localization,
            UiFeedback ui)
        {
            _view = view;
            _navigation = navigation;
            _store = store;
            _shop = shop;
            _inventory = inventory;
            _progress = progress;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            foreach (var laser in _store.Lasers)
            {
                var card = _view.AddItem(ShopTab.Lasers);
                _laserCards.Add(card);
                Listen(card, () => OnLaserClicked(laser, card));
            }

            if (_store.SpareBattery != null)
                _gear.Add(_store.SpareBattery);
            foreach (var booster in _store.Boosters)
                _gear.Add(booster);
            foreach (var gear in _gear)
            {
                var card = _view.AddItem(ShopTab.Gear);
                _gearCards.Add(card);
                Listen(card, () => OnGearClicked(gear, card));
            }

            _navigation.Current.Changed += OnScreenChanged;
            _navigation.CurrentShopTab.Changed += OnTabChanged;
            _inventory.Changed += Render;
            _progress.Ectoplasm.Changed += OnEctoplasmChanged;
            _localization.Changed += Render;
            _view.BackClicked += OnBackClicked;
            _view.TabClicked += OnTabClicked;

            OnScreenChanged(_navigation.Current.Value);
            _view.SetTab(_navigation.CurrentShopTab.Value);
            _view.SetBalance(_progress.Ectoplasm.Value);
            Render();
        }

        public void Dispose()
        {
            for (var i = 0; i < _handled.Count; i++)
                _handled[i].ActionClicked -= _handlers[i];

            _navigation.Current.Changed -= OnScreenChanged;
            _navigation.CurrentShopTab.Changed -= OnTabChanged;
            _inventory.Changed -= Render;
            _progress.Ectoplasm.Changed -= OnEctoplasmChanged;
            _localization.Changed -= Render;
            _view.BackClicked -= OnBackClicked;
            _view.TabClicked -= OnTabClicked;
        }

        private void Listen(ShopItemView card, Action handler)
        {
            card.ActionClicked += handler;
            _handled.Add(card);
            _handlers.Add(handler);
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            _view.SetVisible(screen == MenuScreen.Shop);
        }

        private void OnTabChanged(ShopTab tab)
        {
            _view.SetTab(tab);
        }

        private void OnTabClicked(ShopTab tab)
        {
            if (tab == _navigation.CurrentShopTab.Value)
                return;

            _ui.PlayClick();
            _navigation.ShowShop(tab);
        }

        private void OnEctoplasmChanged(int amount)
        {
            _view.SetBalance(amount);
            Render();
        }

        private void Render()
        {
            for (var i = 0; i < _laserCards.Count; i++)
                RenderLaser(_store.Lasers[i], _laserCards[i]);
            for (var i = 0; i < _gearCards.Count; i++)
                RenderGear(_gear[i], _gearCards[i]);
        }

        private void RenderLaser(LaserData laser, ShopItemView card)
        {
            card.SetContent(laser.Icon, laser.BeamColor,
                _localization.Get(LocalizationTable.Store, laser.NameKey),
                _localization.Get(LocalizationTable.Store, laser.DescriptionKey));
            card.SetStats(laser.PowerRating, laser.HoldRating, laser.EconomyRating);
            card.SetCount(string.Empty);

            if (_inventory.EquippedLaserId.Value == laser.Id)
                card.SetAction(_localization.Get(LocalizationTable.Ui, EquippedKey), ShopItemState.Equipped);
            else if (_inventory.OwnsLaser(laser.Id))
                card.SetAction(_localization.Get(LocalizationTable.Ui, EquipKey), ShopItemState.Equip);
            else
                card.SetAction(_localization.Get(LocalizationTable.Ui, PriceKey, laser.Price),
                    _shop.CanAfford(laser.Price) ? ShopItemState.Buy : ShopItemState.CantAfford);
        }

        private void RenderGear(GearData gear, ShopItemView card)
        {
            card.SetContent(gear.Icon, gear.Accent,
                _localization.Get(LocalizationTable.Store, gear.NameKey),
                _localization.Get(LocalizationTable.Store, gear.DescriptionKey));
            card.HideStats();

            var count = _inventory.GetCount(gear.Id);
            card.SetCount(count > 0 ? _localization.Get(LocalizationTable.Ui, OwnedKey, count, gear.MaxStack) : string.Empty);
            if (count >= gear.MaxStack)
                card.SetAction(_localization.Get(LocalizationTable.Ui, MaxKey), ShopItemState.Full);
            else
                card.SetAction(_localization.Get(LocalizationTable.Ui, PriceKey, gear.Price),
                    _shop.CanAfford(gear.Price) ? ShopItemState.Buy : ShopItemState.CantAfford);
        }

        private void OnLaserClicked(LaserData laser, ShopItemView card)
        {
            if (_inventory.EquippedLaserId.Value == laser.Id)
                return;

            if (_inventory.OwnsLaser(laser.Id))
            {
                _ui.PlayClick();
                _shop.EquipLaser(laser);
                return;
            }

            Show(_shop.BuyLaser(laser), card);
        }

        private void OnGearClicked(GearData gear, ShopItemView card)
        {
            Show(_shop.BuyGear(gear), card);
        }

        private void Show(PurchaseResult result, ShopItemView card)
        {
            if (result == PurchaseResult.Purchased)
            {
                _ui.PlayPurchase();
                card.PlayPurchased();
                return;
            }

            _ui.PlayDenied();
            card.PlayDenied();
        }

        private void OnBackClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }
    }
}
