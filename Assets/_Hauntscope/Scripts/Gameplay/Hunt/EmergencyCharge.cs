using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Store;
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
        private readonly ObservableValue<EmergencyState> _state = new ObservableValue<EmergencyState>(EmergencyState.Idle);
        private readonly ObservableValue<float> _timeLeft = new ObservableValue<float>();

        public EmergencyCharge(HuntPause pause, SpareBatteries spares, EmergencyConfig config)
        {
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

        public bool TryOffer()
        {
            if (_state.Value != EmergencyState.Idle)
                return true;
            if (IsDeclined || !CanUseSpare)
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
