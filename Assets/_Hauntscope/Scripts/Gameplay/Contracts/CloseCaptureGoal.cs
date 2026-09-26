using System;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // A catch made up close.
    [Serializable]
    public sealed class CloseCaptureGoal : ContractGoal
    {
        [SerializeField] private float _maxDistance = 1f;

        public CloseCaptureGoal()
        {
        }

        public CloseCaptureGoal(float maxDistance)
        {
            _maxDistance = maxDistance;
        }

        public override int Count(HuntReport report, string subject)
        {
            return report.IsCaptured && report.CaptureDistance <= _maxDistance ? 1 : 0;
        }
    }
}
