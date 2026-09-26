using System;
using Hauntscope.Gameplay.Hunt;

namespace Hauntscope.Gameplay.Contracts
{
    // Any catch counts.
    [Serializable]
    public sealed class CaptureCountGoal : ContractGoal
    {
        public override int Count(HuntReport report, string subject)
        {
            return report.IsCaptured ? 1 : 0;
        }
    }
}
