using System.Collections.Generic;
using Hauntscope.Gameplay.Contracts;

namespace Hauntscope.Gameplay.Engagement
{
    // The daily side of the game that is saved: today's contract board and the replaces used on it, and how far the
    // daily ration's cassette has run.
    public sealed class EngagementProgress
    {
        private readonly List<ContractSlot> _contracts;

        public EngagementProgress() : this(0, new List<ContractSlot>(), 0, 0)
        {
        }

        public EngagementProgress(int contractDay, List<ContractSlot> contracts, int freeReplacesUsed, int adReplacesUsed,
            int loginClaims = 0, int lastLoginDay = 0, int lastLoginShownDay = 0)
        {
            ContractDay = contractDay;
            _contracts = contracts;
            FreeReplacesUsed = freeReplacesUsed;
            AdReplacesUsed = adReplacesUsed;
            LoginClaims = loginClaims;
            LastLoginDay = lastLoginDay;
            LastLoginShownDay = lastLoginShownDay;
        }

        // Rations claimed so far; the cassette frame is this modulo its length.
        public int LoginClaims { get; private set; }

        // yyyymmdd of the last claimed ration.
        public int LastLoginDay { get; private set; }

        // yyyymmdd of the last day the ration card opened by itself.
        public int LastLoginShownDay { get; private set; }

        public void ClaimLogin(int day)
        {
            LoginClaims++;
            LastLoginDay = day;
        }

        public void MarkLoginShown(int day)
        {
            LastLoginShownDay = day;
        }

        public int ContractDay { get; private set; }

        public IReadOnlyList<ContractSlot> Contracts => _contracts;

        public int FreeReplacesUsed { get; private set; }

        public int AdReplacesUsed { get; private set; }

        public void StartContractDay(int day, IReadOnlyList<ContractSlot> contracts)
        {
            ContractDay = day;
            _contracts.Clear();
            _contracts.AddRange(contracts);
            FreeReplacesUsed = 0;
            AdReplacesUsed = 0;
        }

        public void ReplaceContract(int index, ContractSlot contract)
        {
            _contracts[index] = contract;
        }

        public void UseFreeReplace()
        {
            FreeReplacesUsed++;
        }

        public void UseAdReplace()
        {
            AdReplacesUsed++;
        }
    }
}
