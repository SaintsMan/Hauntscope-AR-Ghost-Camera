using System;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // A quick catch.
    [Serializable]
    public sealed class FastCaptureGoal : ContractGoal
    {
        [SerializeField] private float _maxSeconds = 90f;

        public FastCaptureGoal()
        {
        }

        public FastCaptureGoal(float maxSeconds)
        {
            _maxSeconds = maxSeconds;
        }

        public override int Count(HuntReport report, string subject)
        {
            return report.IsCaptured && report.Duration <= _maxSeconds ? 1 : 0;
        }
    }
}
