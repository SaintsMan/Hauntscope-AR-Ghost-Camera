using System;
using Hauntscope.Gameplay.Hunt;

namespace Hauntscope.Gameplay.Contracts
{
    // Ghosts driven out of their hiding spot.
    [Serializable]
    public sealed class FlushOutGoal : ContractGoal
    {
        public override int Count(HuntReport report, string subject)
        {
            return report.FlushOuts;
        }
    }
}
