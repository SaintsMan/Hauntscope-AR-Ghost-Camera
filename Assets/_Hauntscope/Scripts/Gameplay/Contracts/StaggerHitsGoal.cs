using System;
using Hauntscope.Gameplay.Hunt;

namespace Hauntscope.Gameplay.Contracts
{
    // Beam hits on a vulnerable ghost.
    [Serializable]
    public sealed class StaggerHitsGoal : ContractGoal
    {
        public override int Count(HuntReport report, string subject)
        {
            return report.StaggerHits;
        }
    }
}
