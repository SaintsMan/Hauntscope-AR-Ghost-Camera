using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Season;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // The skin shelf: tapping a row buys the skin, wears it, or takes it off, as that row says. A skin the purse cannot
    // pay for shakes and keeps its price on show, so the next hunts have something to aim at.
    public sealed class HalloweenPresenter : IStartable, IDisposable
    {
        private const string CandyKey = "halloween.count";
        private const string DaysKey = "halloween.banner.days";
        private const string LastDayKey = "halloween.banner.last_day";
        private const string HintKey = "halloween.hint";
        private const string HintOverKey = "halloween.hint_over";
        private const string SlotKeyPrefix = "halloween.slot.";
        private const string BuyKey = "halloween.state.buy";
        private const string OwnedKey = "halloween.state.owned";
        private const string WornKey = "halloween.state.worn";
        private const string OverKey = "halloween.state.over";

        private readonly HalloweenView _view;
        private readonly HalloweenCollection _collection;
        private readonly MenuNavigation _navigation;
        private readonly ILocalizationService _localization;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly AudioConfig _audio;
        private readonly UiFeedback _ui;

        public HalloweenPresenter(HalloweenView view, HalloweenCollection collection, MenuNavigation navigation, ILocalizationService localization,
            ISfxPlayer sfx, IHaptics haptics, AudioConfig audio, UiFeedback ui)
        {
            _view = view;
            _collection = collection;
            _navigation = navigation;
            _localization = localization;
            _sfx = sfx;
            _haptics = haptics;
            _audio = audio;
            _ui = ui;
        }

        public void Start()
        {
            _navigation.Current.Changed += OnScreenChanged;
            _collection.Candy.Changed += OnCandyChanged;
            _collection.Worn += Render;
            _localization.Changed += Render;
            _view.BackClicked += OnBackClicked;
            _view.RowClicked += OnRowClicked;
            OnScreenChanged(_navigation.Current.Value);
        }

        public void Dispose()
        {
            _navigation.Current.Changed -= OnScreenChanged;
            _collection.Candy.Changed -= OnCandyChanged;
            _collection.Worn -= Render;
            _localization.Changed -= Render;
            _view.BackClicked -= OnBackClicked;
            _view.RowClicked -= OnRowClicked;
        }

        private bool IsShown => _navigation.Current.Value == MenuScreen.Halloween;

        private void OnScreenChanged(MenuScreen screen)
        {
            _view.SetVisible(IsShown);
            Render();
        }

        private void OnCandyChanged(int candy)
        {
            Render();
        }

        private void OnBackClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }

        private void OnRowClicked(int row)
        {
            var skins = _collection.Skins;
            if (row >= skins.Length)
                return;

            var skin = skins[row];
            if (_collection.Owns(skin))
            {
                _ui.PlayClick();
                _collection.Toggle(skin);
                return;
            }

            var result = _collection.Buy(skin);
            if (result == SkinPurchase.Bought)
            {
                _sfx.Play2D(_audio.CandyChime, _audio.CandyChimeVolume, 1f);
                _haptics.Play(HapticStrength.Medium);
                _view.Row(row).PlayBought();
                return;
            }

            _ui.PlayDenied();
            _view.Row(row).PlayDenied();
        }

        private void Render()
        {
            if (!IsShown)
                return;

            var left = _collection.DaysLeft;
            var days = !_collection.IsRunning ? string.Empty : left > 0 ? Ui(DaysKey, left) : Ui(LastDayKey);
            _view.SetHeader(Ui(CandyKey, _collection.Candy.Value), days, Ui(_collection.IsRunning ? HintKey : HintOverKey));
            var skins = _collection.Skins;
            for (var row = 0; row < _view.RowCount; row++)
            {
                var view = _view.Row(row);
                var shown = row < skins.Length;
                view.gameObject.SetActive(shown);
                if (!shown)
                    continue;

                RenderRow(view, skins[row]);
            }
        }

        private void RenderRow(SkinRowView view, HalloweenSkin skin)
        {
            var slot = Ui(SlotKeyPrefix + SnakeCase(skin.Slot));
            var name = Ui(skin.NameKey);
            if (_collection.IsWorn(skin))
                view.SetContent(slot, name, Ui(WornKey), skin.Icon, SkinRowState.Worn);
            else if (_collection.Owns(skin))
                view.SetContent(slot, name, Ui(OwnedKey), skin.Icon, SkinRowState.Owned);
            else if (!_collection.IsRunning)
                view.SetContent(slot, name, Ui(OverKey), skin.Icon, SkinRowState.Locked);
            else
                view.SetContent(slot, name, Ui(BuyKey, _collection.PriceOf(skin)), skin.Icon, SkinRowState.Buyable);
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }

        private static string SnakeCase(HalloweenSkinSlot slot)
        {
            return AnalyticsNames.Of(slot);
        }
    }
}
