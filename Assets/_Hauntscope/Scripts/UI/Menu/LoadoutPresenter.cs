using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Store;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // Boosters toggle in and out of the next hunt; the spare battery is always carried, and an empty slot opens the shop.
    public sealed class LoadoutPresenter : IStartable, IDisposable
    {
        private const string CountKey = "loadout.count";

        private readonly LoadoutView _view;
        private readonly StoreConfig _store;
        private readonly PlayerInventory _inventory;
        private readonly Shop _shop;
        private readonly MenuNavigation _navigation;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly List<GearData> _gear = new List<GearData>();
        private readonly List<LoadoutSlotView> _slots = new List<LoadoutSlotView>();
        private readonly List<Action> _handlers = new List<Action>();

        public LoadoutPresenter(
            LoadoutView view,
            StoreConfig store,
            PlayerInventory inventory,
            Shop shop,
            MenuNavigation navigation,
            ILocalizationService localization,
            UiFeedback ui)
        {
            _view = view;
            _store = store;
            _inventory = inventory;
            _shop = shop;
            _navigation = navigation;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            if (_store.SpareBattery != null)
                _gear.Add(_store.SpareBattery);
            foreach (var booster in _store.Boosters)
                _gear.Add(booster);

            foreach (var gear in _gear)
            {
                var slot = _view.AddSlot();
                slot.SetGear(gear.Icon, gear.Accent);
                Action handler = () => OnSlotClicked(gear, slot);
                slot.Clicked += handler;
                _slots.Add(slot);
                _handlers.Add(handler);
            }

            _inventory.Changed += Render;
            _localization.Changed += Render;
            Render();
        }

        public void Dispose()
        {
            for (var i = 0; i < _slots.Count; i++)
                _slots[i].Clicked -= _handlers[i];

            _inventory.Changed -= Render;
            _localization.Changed -= Render;
        }

        private void Render()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var gear = _gear[i];
                var count = _inventory.GetCount(gear.Id);
                _slots[i].SetState(_localization.Get(LocalizationTable.Ui, CountKey, count), count == 0, count > 0 && IsTaken(gear));
            }
        }

        private bool IsTaken(GearData gear)
        {
            return gear == _store.SpareBattery || _inventory.IsArmed(gear.Id);
        }

        private void OnSlotClicked(GearData gear, LoadoutSlotView slot)
        {
            if (_inventory.GetCount(gear.Id) == 0 || gear == _store.SpareBattery)
            {
                _ui.PlayClick();
                _navigation.ShowShop(ShopTab.Gear);
                return;
            }

            var armed = !_inventory.IsArmed(gear.Id);
            _ui.PlayClick();
            _shop.SetArmed(gear, armed);
            slot.PlayToggle(armed);
        }
    }
}
