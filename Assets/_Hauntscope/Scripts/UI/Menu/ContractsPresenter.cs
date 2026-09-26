using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Contracts;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Progress;
using Hauntscope.UI.Common;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class ContractsPresenter : IStartable, IDisposable
    {
        private const string OrderKey = "contracts.order";
        private const string ProgressKey = "contracts.progress";
        private const string DoneKey = "contracts.done";
        private const string PaidKey = "contracts.paid";
        private const string ClaimedKey = "contracts.claimed";
        private const string RewardKey = "contracts.reward";
        private const string GearRewardKey = "contracts.reward_gear";
        private const string ReplaceKey = "contracts.replace";
        private const string CountdownKey = "contracts.countdown";
        private const int DayOfYearDigits = 10000;
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 3600;

        private static readonly string[] TierKeys = { "contracts.tier.easy", "contracts.tier.medium", "contracts.tier.hard" };

        private readonly ContractsView _view;
        private readonly ContractBoard _board;
        private readonly MenuNavigation _navigation;
        private readonly PlayerProgress _progress;
        private readonly ContractDescriber _describer;
        private readonly ILocalizationService _localization;
        private readonly IClock _clock;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private bool _isShown;

        public ContractsPresenter(ContractsView view, ContractBoard board, MenuNavigation navigation, PlayerProgress progress,
            ContractDescriber describer, ILocalizationService localization, IClock clock, UiFeedback ui)
        {
            _view = view;
            _board = board;
            _navigation = navigation;
            _progress = progress;
            _describer = describer;
            _localization = localization;
            _clock = clock;
            _ui = ui;
        }

        public void Start()
        {
            _board.Refresh();
            _board.Changed += Render;
            _navigation.Current.Changed += OnScreenChanged;
            _progress.Ectoplasm.Changed += OnEctoplasmChanged;
            _localization.Changed += Render;
            _view.BackClicked += OnBackClicked;
            _view.ClaimClicked += OnClaimClicked;
            _view.ReplaceClicked += OnReplaceClicked;
            _view.SetBalance(_progress.Ectoplasm.Value);
            OnScreenChanged(_navigation.Current.Value);
            TickCountdownAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _board.Changed -= Render;
            _navigation.Current.Changed -= OnScreenChanged;
            _progress.Ectoplasm.Changed -= OnEctoplasmChanged;
            _localization.Changed -= Render;
            _view.BackClicked -= OnBackClicked;
            _view.ClaimClicked -= OnClaimClicked;
            _view.ReplaceClicked -= OnReplaceClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        // Opening the screen after midnight deals the new orders there and then.
        private void OnScreenChanged(MenuScreen screen)
        {
            var shown = screen == MenuScreen.Contracts;
            _view.SetVisible(shown);
            if (shown && !_isShown)
                _board.Refresh();
            _isShown = shown;
            if (shown)
                Render();
        }

        private void OnEctoplasmChanged(int amount)
        {
            _view.SetBalance(amount);
        }

        private void OnBackClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }

        private void OnClaimClicked(int index)
        {
            if (_board.Claim(index) < 0)
                return;

            _ui.PlayReward();
            _view.PlayClaimed(index);
        }

        private void OnReplaceClicked(int index)
        {
            if (!_board.CanReplace(index))
            {
                _ui.PlayDenied();
                return;
            }

            if (_board.HasFreeReplace)
            {
                if (!_board.ReplaceFree(index))
                    return;

                _ui.PlayClick();
                _view.Card(index).PlayReplaced();
                return;
            }

            _ui.PlayClick();
            ReplaceWithAdAsync(index, _lifetime.Token).Forget();
        }

        private async UniTaskVoid ReplaceWithAdAsync(int index, CancellationToken cancellationToken)
        {
            if (await _board.ReplaceWithAdAsync(index, cancellationToken))
                _view.Card(index).PlayReplaced();
        }

        private void Render()
        {
            if (!_isShown)
                return;

            var slots = _board.Slots;
            for (var i = 0; i < _view.CardCount; i++)
            {
                var card = _view.Card(i);
                if (i >= slots.Count)
                {
                    card.SetVisible(false);
                    continue;
                }

                card.SetVisible(true);
                RenderCard(card, slots[i], i);
            }
        }

        private void RenderCard(ContractCardView card, ContractSlot slot, int index)
        {
            var data = slot.Data;
            card.SetHeader(Ui(OrderKey, _board.Day % DayOfYearDigits, index + 1), Ui(TierKeys[Mathf.Clamp((int)data.Tier, 0, TierKeys.Length - 1)]),
                data.Tier);
            card.SetDescription(_describer.Describe(slot));
            card.SetProgress((float)slot.Progress / slot.Target, Ui(ProgressKey, slot.Progress, slot.Target));

            var gear = slot.Reward.Gear;
            var reward = slot.IsClaimed
                ? Ui(ClaimedKey)
                : gear != null
                    ? Ui(GearRewardKey, _localization.Get(LocalizationTable.Store, gear.NameKey), slot.Reward.GearCount)
                    : Ui(RewardKey, slot.Reward.Ectoplasm);
            card.SetReward(gear != null ? gear.Icon : null, reward);

            card.SetClaim(slot.IsReady);
            var free = _board.HasFreeReplace;
            var canReplace = !slot.IsComplete && (free || _board.HasAdReplace);
            card.SetReplace(canReplace, !_board.IsReplacing && (free || _board.IsAdReady), !free, Ui(ReplaceKey));
            card.SetStamp(slot.IsComplete, Ui(slot.IsClaimed ? PaidKey : DoneKey), slot.IsClaimed, true);
        }

        // Counts down to local midnight, when the next board is dealt.
        private async UniTaskVoid TickCountdownAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_isShown)
                {
                    var left = (int)(_clock.Today.AddDays(1) - _clock.LocalNow).TotalSeconds;
                    left = Mathf.Max(0, left);
                    _view.SetCountdown(Ui(CountdownKey, left / SecondsPerHour, left % SecondsPerHour / SecondsPerMinute, left % SecondsPerMinute));
                    if (left == 0)
                        _board.Refresh();
                }

                if (await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken).SuppressCancellationThrow())
                    return;
            }
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }
    }
}
