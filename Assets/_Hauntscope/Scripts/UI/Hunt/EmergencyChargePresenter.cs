using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class EmergencyChargePresenter : IStartable, IDisposable
    {
        private const string SpareKey = "emergency.spare";
        private const string SecondsKey = "emergency.seconds";

        private readonly EmergencyChargeView _view;
        private readonly EmergencyCharge _emergency;
        private readonly SpareBatteries _spares;
        private readonly ILocalizationService _localization;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly AudioConfig _audio;
        private readonly UiFeedback _ui;
        private readonly Func<int, string> _formatSeconds;
        private readonly IAdsService _ads;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public EmergencyChargePresenter(
            EmergencyChargeView view,
            EmergencyCharge emergency,
            SpareBatteries spares,
            ILocalizationService localization,
            ISfxPlayer sfx,
            IHaptics haptics,
            AudioConfig audio,
            UiFeedback ui,
            IAdsService ads)
        {
            _ads = ads;
            _view = view;
            _emergency = emergency;
            _spares = spares;
            _localization = localization;
            _sfx = sfx;
            _haptics = haptics;
            _audio = audio;
            _ui = ui;
            _formatSeconds = FormatSeconds;
        }

        public void Start()
        {
            _emergency.State.Changed += OnStateChanged;
            _emergency.TimeLeft.Changed += OnTimeLeftChanged;
            _localization.Changed += Render;
            _view.SpareClicked += OnSpareClicked;
            _view.GiveUpClicked += OnGiveUpClicked;
            _view.AdClicked += OnAdClicked;
            _ads.AvailabilityChanged += Render;
            _view.SetVisible(false);
        }

        public void Dispose()
        {
            _emergency.State.Changed -= OnStateChanged;
            _emergency.TimeLeft.Changed -= OnTimeLeftChanged;
            _localization.Changed -= Render;
            _view.SpareClicked -= OnSpareClicked;
            _view.GiveUpClicked -= OnGiveUpClicked;
            _view.AdClicked -= OnAdClicked;
            _ads.AvailabilityChanged -= Render;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnStateChanged(EmergencyState state)
        {
            if (state == EmergencyState.Offered)
            {
                _sfx.Play2D(_audio.EmergencyAlarm, _audio.EmergencyVolume, 1f);
                _haptics.Play(HapticStrength.Heavy);
            }

            Render();
        }

        private void OnTimeLeftChanged(float timeLeft)
        {
            RenderCountdown();
        }

        private void Render()
        {
            var state = _emergency.State.Value;
            _view.SetVisible(state != EmergencyState.Idle);
            if (state == EmergencyState.Idle)
                return;

            _view.SetSpare(_emergency.CanUseSpare, _localization.Get(LocalizationTable.Ui, SpareKey, _spares.Count));
            _view.SetAd(_emergency.IsAdOffered, _emergency.CanWatchAd && state == EmergencyState.Offered);
            RenderCountdown();
        }

        private void RenderCountdown()
        {
            var timeLeft = _emergency.TimeLeft.Value;
            _view.SetCountdown(timeLeft / _emergency.Countdown, Mathf.CeilToInt(timeLeft), _formatSeconds);
        }

        private string FormatSeconds(int seconds)
        {
            return _localization.Get(LocalizationTable.Ui, SecondsKey, seconds);
        }

        private void OnSpareClicked()
        {
            _emergency.UseSpare();
        }

        private void OnAdClicked()
        {
            _ui.PlayClick();
            WatchAdAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid WatchAdAsync(CancellationToken cancellationToken)
        {
            if (await _emergency.WatchAdAsync(cancellationToken))
                _ui.PlayReward();
        }

        private void OnGiveUpClicked()
        {
            _ui.PlayBack();
            _emergency.GiveUp();
        }
    }
}
