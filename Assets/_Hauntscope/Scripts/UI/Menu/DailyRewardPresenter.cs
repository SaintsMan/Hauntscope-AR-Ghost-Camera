using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Feedback;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // The daily ration (GDD 5.30): opens by itself on the first menu of a day through the popup queue, and any time
    // from its icon.
    public sealed class DailyRewardPresenter : IStartable, IDisposable, IMenuPopup
    {
        private const string DayKey = "ration.day";
        private const string SubtitleKey = "ration.subtitle";
        private const string HintKey = "ration.hint";
        private const string TomorrowKey = "ration.tomorrow";
        private const string ClaimKey = "ration.claim";
        private const string DoneKey = "ration.done";
        private const string DoubleKey = "ration.double";
        private const string EctoplasmKey = "ration.ectoplasm";
        private const string GearKey = "ration.gear";
        private const int MaxItems = 2;

        private readonly DailyRewardView _view;
        private readonly CalendarButtonView _button;
        private readonly LoginCalendar _calendar;
        private readonly MenuPopupQueue _popups;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private bool _isOpen;

        public DailyRewardPresenter(DailyRewardView view, CalendarButtonView button, LoginCalendar calendar, MenuPopupQueue popups,
            ILocalizationService localization, UiFeedback ui)
        {
            _view = view;
            _button = button;
            _calendar = calendar;
            _popups = popups;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _view.SetVisible(false);
            _calendar.Changed += Render;
            _localization.Changed += Render;
            _button.Clicked += OnIconClicked;
            _view.ClaimClicked += OnClaimClicked;
            _view.DoubleClicked += OnDoubleClicked;
            _view.CloseClicked += OnCloseClicked;
            Render();
            if (_calendar.ShouldOpenByItself)
                _popups.Enqueue(this);
        }

        public void Dispose()
        {
            _calendar.Changed -= Render;
            _localization.Changed -= Render;
            _button.Clicked -= OnIconClicked;
            _view.ClaimClicked -= OnClaimClicked;
            _view.DoubleClicked -= OnDoubleClicked;
            _view.CloseClicked -= OnCloseClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public void Open()
        {
            if (_calendar.CanClaim)
                _calendar.MarkShown();
            _isOpen = true;
            _view.SetVisible(true);
            Render();
        }

        public void Close()
        {
            if (!_isOpen)
                return;

            _isOpen = false;
            _view.SetVisible(false);
            _popups.NotifyClosed(this);
        }

        private void OnIconClicked()
        {
            _ui.PlayClick();
            _popups.Enqueue(this);
        }

        private void OnCloseClicked()
        {
            _ui.PlayBack();
            Close();
        }

        // Once today's frame is claimed the same button just closes the card.
        private void OnClaimClicked()
        {
            if (!_calendar.CanClaim)
            {
                _ui.PlayClick();
                Close();
                return;
            }

            if (_calendar.Claim() != null)
                PlayClaimed();
        }

        private void OnDoubleClicked()
        {
            _ui.PlayClick();
            ClaimDoubledAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid ClaimDoubledAsync(CancellationToken cancellationToken)
        {
            if (await _calendar.ClaimDoubledAsync(cancellationToken) != null)
                PlayClaimed();
        }

        private void PlayClaimed()
        {
            _ui.PlayReward();
            _view.Cell(_calendar.TodayIndex).PlayClaimed();
        }

        private void Render()
        {
            _button.SetBadge(_calendar.CanClaim);
            if (!_isOpen || _calendar.Length == 0)
                return;

            var today = _calendar.TodayIndex;
            var claimed = _calendar.IsClaimedToday;
            for (var i = 0; i < _view.CellCount; i++)
            {
                var cell = _view.Cell(i);
                var shown = i < _calendar.Length;
                cell.gameObject.SetActive(shown);
                if (!shown)
                    continue;

                cell.SetDay(Ui(DayKey, i + 1));
                RenderItems(cell, _calendar.Reward(i).ToBundle());
                cell.SetState(i < today || (i == today && claimed) ? RationCellState.Claimed
                    : i == today ? RationCellState.Today : RationCellState.Future);
            }

            _view.SetText(Ui(SubtitleKey, today + 1, _calendar.Length), Ui(claimed ? TomorrowKey : HintKey));
            _view.SetClaim(Ui(claimed ? DoneKey : ClaimKey));
            _view.SetDouble(_calendar.IsDoubleOffered, _calendar.CanDouble, Ui(DoubleKey, _calendar.AdMultiplier));
        }

        private void RenderItems(DailyRewardCellView cell, RewardBundle bundle)
        {
            var count = 0;
            if (bundle.Ectoplasm > 0)
                cell.SetItem(count++, null, Ui(EctoplasmKey, bundle.Ectoplasm));
            foreach (var item in bundle.Items)
            {
                if (count == MaxItems)
                    break;
                cell.SetItem(count++, item.Gear.Icon, Ui(GearKey, item.Count));
            }

            cell.SetItemCount(count);
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }
    }
}
