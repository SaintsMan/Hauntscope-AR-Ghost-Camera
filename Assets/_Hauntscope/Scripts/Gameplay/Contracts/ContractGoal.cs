using System;
using Hauntscope.Gameplay.Hunt;

namespace Hauntscope.Gameplay.Contracts
{
    // A contract's condition (GDD 5.29). Goals hold only their settings; progress lives in the contract slot, so one
    // goal serves every day's contract of its kind.
    [Serializable]
    public abstract class ContractGoal
    {
        // Whether the contract can be offered now, and what it is about (a ghost id for "catch {ghost}", else empty).
        public virtual bool TrySelectSubject(ContractContext context, out string subject)
        {
            subject = string.Empty;
            return true;
        }

        // How far one hunt moves the contract.
        public abstract int Count(HuntReport report, string subject);
    }
}
