using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Promo;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // Settings → "Promo code": type a code, the Bureau checks it with the studio's server and hands out the gift with the
    // studio's own message. Errors stay on the card so the player can fix a typo and try again.
    public sealed class PromoPresenter : IStartable, IDisposable
    {
        private const string HintKey = "promo.hint";
        private const string PlaceholderKey = "promo.placeholder";
        private const string ActivateKey = "promo.activate";
        private const string CheckingKey = "promo.checking";
        private const string ClaimKey = "promo.claim";
        private const string ErrorKeyPrefix = "promo.error.";
        private const string FailedErrorKey = "promo.error.failed";
        private const string NoRewardKey = "promo.reward.none";
        private const string EctoplasmKey = "promo.reward.ectoplasm";
        private const string FilmKey = "promo.reward.film";
        private const string GearKey = "promo.reward.gear";
        private const string PremiumKey = "promo.reward.premium";

        private readonly PromoView _view;
        private readonly MenuNavigation _navigation;
        private readonly PromoRedeemer _redeemer;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly StoreConfig _store;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private bool _isChecking;
        private string _errorKey;
        private PromoResult _gift;

        public PromoPresenter(PromoView view, MenuNavigation navigation, PromoRedeemer redeemer, ILocalizationService localization,
            UiFeedback ui, StoreConfig store)
        {
            _view = view;
            _navigation = navigation;
            _redeemer = redeemer;
            _localization = localization;
            _ui = ui;
            _store = store;
        }

        public void Start()
        {
            _navigation.Current.Changed += OnScreenChanged;
            _localization.Changed += Render;
            _view.ActivateClicked += OnActivateClicked;
            _view.CloseClicked += OnCloseClicked;
            _view.CodeChanged += OnCodeChanged;
            OnScreenChanged(_navigation.Current.Value);
        }

        public void Dispose()
        {
            _navigation.Current.Changed -= OnScreenChanged;
            _localization.Changed -= Render;
            _view.ActivateClicked -= OnActivateClicked;
            _view.CloseClicked -= OnCloseClicked;
            _view.CodeChanged -= OnCodeChanged;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            var visible = screen == MenuScreen.Promo;
            _view.SetVisible(visible);
            if (!visible || _isChecking)
                return;

            _gift = null;
            _errorKey = null;
            _view.SetCode(string.Empty);
            Render();
        }

        // Shown in the field the way the server reads it: capitals, no spaces.
        private void OnCodeChanged(string text)
        {
            var code = _redeemer.Normalize(text);
            if (code != text)
                _view.SetCode(code);

            _errorKey = null;
            Render();
        }

        private void OnActivateClicked()
        {
            if (_gift != null)
            {
                OnCloseClicked();
                return;
            }

            if (_isChecking || _redeemer.Normalize(_view.Code).Length == 0)
                return;

            _ui.PlayClick();
            RedeemAsync(_lifetime.Token).Forget();
        }

        private void OnCloseClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }

        private async UniTaskVoid RedeemAsync(CancellationToken cancellationToken)
        {
            _isChecking = true;
            _errorKey = null;
            Render();
            PromoResult result;
            try
            {
                result = await _redeemer.RedeemAsync(_view.Code, cancellationToken);
            }
            finally
            {
                _isChecking = false;
            }

            if (result.Ok)
            {
                _gift = result;
                _ui.PlayReward();
            }
            else
            {
                _errorKey = ErrorKey(result.Error);
                _ui.PlayDenied();
            }

            Render();
        }

        private void Render()
        {
            if (_gift != null)
            {
                _view.SetInputVisible(false);
                _view.SetCloseVisible(false);
                _view.SetBody(GiftText(_gift));
                _view.SetActivate(Ui(ClaimKey), true);
                return;
            }

            _view.SetInputVisible(true);
            _view.SetCloseVisible(true);
            _view.SetInputInteractable(!_isChecking);
            _view.SetPlaceholder(Ui(PlaceholderKey));
            _view.SetBody(Ui(_isChecking ? CheckingKey : _errorKey ?? HintKey));
            _view.SetActivate(Ui(ActivateKey), !_isChecking && _redeemer.Normalize(_view.Code).Length > 0);
        }

        // The studio's message, then what landed in the agent's kit. A code this install already cashed in pays nothing again.
        private string GiftText(PromoResult gift)
        {
            var text = new StringBuilder(gift.Message);
            if (text.Length > 0)
                text.Append("\n\n");

            if (gift.Rewards.Count == 0)
                text.Append(Ui(NoRewardKey));
            for (var i = 0; i < gift.Rewards.Count; i++)
            {
                if (i > 0)
                    text.Append('\n');
                text.Append(RewardLine(gift.Rewards[i]));
            }

            return text.ToString();
        }

        private string RewardLine(PromoReward reward)
        {
            switch (reward.Type)
            {
                case PromoRewardTypes.Ectoplasm:
                    return Ui(EctoplasmKey, reward.Amount);
                case PromoRewardTypes.Film:
                    return Ui(FilmKey, reward.Amount);
                case PromoRewardTypes.Gear:
                    var gear = _store.FindGear(reward.Id);
                    return Ui(GearKey, reward.Amount, gear != null ? _localization.Get(LocalizationTable.Store, gear.NameKey) : reward.Id);
                default:
                    return Ui(PremiumKey);
            }
        }

        // Server codes that mean the same to a player share one text.
        private static string ErrorKey(string error)
        {
            switch (error)
            {
                case PromoErrors.NotFound:
                case PromoErrors.OtherGame:
                case PromoErrors.Used:
                case PromoErrors.NotStarted:
                case PromoErrors.Limit:
                case PromoErrors.Rate:
                case PromoErrors.Offline:
                case PromoErrors.Inactive:
                    return ErrorKeyPrefix + error;
                case PromoErrors.Expired:
                    return ErrorKeyPrefix + PromoErrors.Inactive;
                default:
                    return FailedErrorKey;
            }
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }
    }
}
