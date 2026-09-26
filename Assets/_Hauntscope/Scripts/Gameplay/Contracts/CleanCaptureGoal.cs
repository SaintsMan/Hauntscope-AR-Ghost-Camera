using System;
using Hauntscope.Gameplay.Hunt;

namespace Hauntscope.Gameplay.Contracts
{
    // A catch on the laser alone: no spare battery, no boosters.
    [Serializable]
    public sealed class CleanCaptureGoal : ContractGoal
    {
        public override int Count(HuntReport report, string subject)
        {
            return report.IsCaptured && !report.UsedSpareBattery && !report.UsedBoosters ? 1 : 0;
        }
    }
}
