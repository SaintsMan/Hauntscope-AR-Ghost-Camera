using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    public sealed class Battery
    {
        private readonly ToolsConfig _config;
        private readonly ObservableValue<float> _charge;
        private readonly ObservableValue<bool> _isLow = new ObservableValue<bool>();

        public Battery(ToolsConfig config)
        {
            _config = config;
            _charge = new ObservableValue<float>(config.BatteryMax);
        }

        public IReadOnlyObservableValue<float> Charge => _charge;

        public IReadOnlyObservableValue<bool> IsLow => _isLow;

        public float Normalized => _charge.Value / _config.BatteryMax;

        public bool IsDepleted => _charge.Value <= 0f;

        public void Drain(float amount)
        {
            SetCharge(_charge.Value - amount);
        }

        // A share of a full charge gone at once (the mara's bite).
        public void DrainFraction(float fraction)
        {
            SetCharge(_charge.Value - fraction * _config.BatteryMax);
        }

        public void Recharge(float fraction)
        {
            SetCharge(_charge.Value + fraction * _config.BatteryMax);
        }

        public void Refill()
        {
            SetCharge(_config.BatteryMax);
        }

        private void SetCharge(float charge)
        {
            _charge.Value = Mathf.Clamp(charge, 0f, _config.BatteryMax);
            _isLow.Value = Normalized < _config.LowBatteryThreshold;
        }
    }
}
