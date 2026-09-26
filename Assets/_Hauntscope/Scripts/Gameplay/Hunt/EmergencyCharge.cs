using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Hunt
{
    // GDD 5.21: a dead battery pauses the hunt on a countdown card instead of ending it, as long as there is a way
    // to recharge. Ticked by itself, since the hunt it pauses no longer ticks.
    public sealed class EmergencyCharge : ITickable
    {
        private readonly HuntPause _pause;
        private readonly SpareBatteries _spares;
        private readonly EmergencyConfig _config;
        private readonly IAdsService _ads;
        private readonly Battery _battery;
        private readonly ObservableValue<EmergencyState> _state = new ObservableValue<EmergencyState>(EmergencyState.Idle);
        private readonly ObservableValue<float> _timeLeft = new ObservableValue<float>();

        private bool _adUsed;

        public EmergencyCharge(HuntPause pause, SpareBatteries spares, EmergencyConfig config, IAdsService ads, Battery battery)
        {
            _ads = ads;
            _battery = battery;
            _pause = pause;
            _spares = spares;
            _config = config;
        }

        public IReadOnlyObservableValue<EmergencyState> State => _state;

        public IReadOnlyObservableValue<float> TimeLeft => _timeLeft;

        public float Countdown => _config.Countdown;

        // The player gave up (or let the countdown run out) this hunt; the next dead battery lets the ghost go.
        public bool IsDeclined { get; private set; }

        public bool CanUseSpare => _spares.Count > 0;

        // Once per hunt, so an ad never becomes the way to play.
        public bool IsAdOffered => !_adUsed;

        public bool CanWatchAd => IsAdOffered && _ads.IsRewardedReady;

        public bool TryOffer()
        {
            if (_state.Value != EmergencyState.Idle)
                return true;
            if (IsDeclined || (!CanUseSpare && !CanWatchAd))
                return false;

            _timeLeft.Value = _config.Countdown;
            _state.Value = EmergencyState.Offered;
            _pause.SetEmergency(true);
            return true;
        }

        public void UseSpare()
        {
            if (_state.Value == EmergencyState.Offered && _spares.TryUse())
                Resolve();
        }

        // The countdown stops while the ad plays; without a reward the card comes back with the time that was left.
        public async UniTask<bool> WatchAdAsync(CancellationToken cancellationToken)
        {
            if (_state.Value != EmergencyState.Offered || !CanWatchAd)
                return false;

            _state.Value = EmergencyState.WatchingAd;
            var rewarded = await _ads.ShowRewardedAsync(cancellationToken);
            if (_state.Value != EmergencyState.WatchingAd)
                return false;

            if (!rewarded)
            {
                _state.Value = EmergencyState.Offered;
                return false;
            }

            _adUsed = true;
            _battery.Recharge(_config.AdCharge);
            Resolve();
            return true;
        }

        public void GiveUp()
        {
            if (_state.Value == EmergencyState.Idle)
                return;

            IsDeclined = true;
            Resolve();
        }

        public void ResetForHunt()
        {
            IsDeclined = false;
            _adUsed = false;
            if (_state.Value != EmergencyState.Idle)
                Resolve();
        }

        void ITickable.Tick()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (_state.Value != EmergencyState.Offered)
                return;

            _timeLeft.Value = Mathf.Max(0f, _timeLeft.Value - deltaTime);
            if (_timeLeft.Value <= 0f)
                GiveUp();
        }

        private void Resolve()
        {
            _state.Value = EmergencyState.Idle;
            _pause.SetEmergency(false);
        }
    }
}
