using System;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // A catch with charge to spare.
    [Serializable]
    public sealed class BatteryLeftGoal : ContractGoal
    {
        [SerializeField] private float _minCharge = 0.5f;

        public BatteryLeftGoal()
        {
        }

        public BatteryLeftGoal(float minCharge)
        {
            _minCharge = minCharge;
        }

        public override int Count(HuntReport report, string subject)
        {
            return report.IsCaptured && report.BatteryLeft >= _minCharge ? 1 : 0;
        }
    }
}
