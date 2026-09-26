using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Pickups;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class SupplyHudPresenter : IStartable, IDisposable
    {
        private const string CountKey = "loadout.count";
        private const string LootKey = "hud.loot";
        private const int Percent = 100;

        private readonly SupplyHudView _view;
        private readonly SpareBatteries _spares;
        private readonly Battery _battery;
        private readonly HuntLoadout _loadout;
        private readonly ILocalizationService _localization;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly AudioConfig _audio;
        private readonly UiFeedback _ui;
        private readonly HuntLoot _loot;
        private readonly PickupField _pickups;

        private int _shownCount = -1;
        private bool _shownUsable;

        public SupplyHudPresenter(
            SupplyHudView view,
            SpareBatteries spares,
            Battery battery,
            HuntLoadout loadout,
            ILocalizationService localization,
            ISfxPlayer sfx,
            IHaptics haptics,
            AudioConfig audio,
            UiFeedback ui,
            HuntLoot loot,
            PickupField pickups)
        {
            _loot = loot;
            _pickups = pickups;
            _view = view;
            _spares = spares;
            _battery = battery;
            _loadout = loadout;
            _localization = localization;
            _sfx = sfx;
            _haptics = haptics;
            _audio = audio;
            _ui = ui;
        }

        public void Start()
        {
            _view.SpareClicked += OnSpareClicked;
            _spares.Used += OnSpareUsed;
            _battery.Charge.Changed += OnChargeChanged;
            _loadout.Changed += RenderBoosters;
            _localization.Changed += OnLanguageChanged;
            _loot.Ectoplasm.Changed += OnLootChanged;
            _pickups.Collected += OnCollected;
            _pickups.Spawned += OnSpawned;
            RenderSpare(true);
            RenderBoosters();
            RenderLoot(false);
        }

        public void Dispose()
        {
            _view.SpareClicked -= OnSpareClicked;
            _spares.Used -= OnSpareUsed;
            _battery.Charge.Changed -= OnChargeChanged;
            _loadout.Changed -= RenderBoosters;
            _localization.Changed -= OnLanguageChanged;
            _loot.Ectoplasm.Changed -= OnLootChanged;
            _pickups.Collected -= OnCollected;
            _pickups.Spawned -= OnSpawned;
        }

        private void OnSpareClicked()
        {
            if (!_spares.TryUse())
                _ui.PlayDenied();
        }

        private void OnSpareUsed()
        {
            _sfx.Play2D(_audio.BatteryInsert, _audio.BatteryInsertVolume, 1f);
            _haptics.Play(HapticStrength.Medium);
            _view.PlaySpareUsed();
            RenderSpare(true);
        }

        // The charge changes every frame; the button only needs a redraw when it becomes (un)usable.
        private void OnChargeChanged(float charge)
        {
            RenderSpare(false);
        }

        private void OnLanguageChanged()
        {
            RenderSpare(true);
            RenderLoot(false);
        }

        private void OnLootChanged(int amount)
        {
            RenderLoot(amount > 0);
        }

        private void RenderLoot(bool punch)
        {
            var amount = _loot.Ectoplasm.Value;
            _view.SetLoot(amount > 0, _localization.Get(LocalizationTable.Ui, LootKey, amount), punch);
        }

        private void OnCollected(Pickup pickup, PickupGain gain)
        {
            if (string.IsNullOrEmpty(pickup.Data.ToastKey))
                return;

            var percent = Mathf.RoundToInt(gain.Charge * Percent);
            _view.ShowToast(_localization.Get(LocalizationTable.Ui, pickup.Data.ToastKey, gain.Ectoplasm, percent), pickup.Data.Color);
        }

        private void OnSpawned(Pickup pickup)
        {
            if (!string.IsNullOrEmpty(pickup.Data.AnnounceKey))
                _view.ShowToast(_localization.Get(LocalizationTable.Ui, pickup.Data.AnnounceKey), pickup.Data.Color);
        }

        private void RenderSpare(bool force)
        {
            var count = _spares.Count;
            var usable = _spares.CanUse;
            if (!force && count == _shownCount && usable == _shownUsable)
                return;

            _shownCount = count;
            _shownUsable = usable;
            _view.SetSpare(count > 0, usable, _localization.Get(LocalizationTable.Ui, CountKey, count));
        }

        private void RenderBoosters()
        {
            var boosters = _loadout.ActiveBoosters;
            var icons = new Sprite[boosters.Count];
            var colors = new Color[boosters.Count];
            for (var i = 0; i < boosters.Count; i++)
            {
                icons[i] = boosters[i].Icon;
                colors[i] = boosters[i].Accent;
            }

            _view.SetBoosters(icons, colors);
        }
    }
}
