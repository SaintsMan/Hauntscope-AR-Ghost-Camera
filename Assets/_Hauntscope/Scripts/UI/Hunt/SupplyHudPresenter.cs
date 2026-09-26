using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class SupplyHudPresenter : IStartable, IDisposable
    {
        private const string CountKey = "loadout.count";

        private readonly SupplyHudView _view;
        private readonly SpareBatteries _spares;
        private readonly Battery _battery;
        private readonly HuntLoadout _loadout;
        private readonly ILocalizationService _localization;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly AudioConfig _audio;
        private readonly UiFeedback _ui;

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
            UiFeedback ui)
        {
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
            RenderSpare(true);
            RenderBoosters();
        }

        public void Dispose()
        {
            _view.SpareClicked -= OnSpareClicked;
            _spares.Used -= OnSpareUsed;
            _battery.Charge.Changed -= OnChargeChanged;
            _loadout.Changed -= RenderBoosters;
            _localization.Changed -= OnLanguageChanged;
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
