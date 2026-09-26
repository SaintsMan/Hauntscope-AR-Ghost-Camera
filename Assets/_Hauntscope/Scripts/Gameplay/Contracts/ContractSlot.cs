using Hauntscope.Gameplay.Engagement;
using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // A contract on today's board: which one, what it is about, how far along and what it pays.
    public sealed class ContractSlot
    {
        public ContractSlot(ContractData data, string subject, RewardBundle reward, int progress = 0, bool isClaimed = false)
        {
            Data = data;
            Subject = subject ?? string.Empty;
            Reward = reward;
            Progress = Mathf.Clamp(progress, 0, data.Target);
            IsClaimed = isClaimed;
        }

        public ContractData Data { get; }

        public string Subject { get; }

        public RewardBundle Reward { get; }

        public int Progress { get; private set; }

        public int Target => Data.Target;

        public bool IsClaimed { get; private set; }

        public bool IsComplete => Progress >= Target;

        // Done, with the reward still waiting.
        public bool IsReady => IsComplete && !IsClaimed;

        // True when this is what completed it.
        public bool Advance(int amount)
        {
            if (IsComplete || amount <= 0)
                return false;

            Progress = Mathf.Min(Target, Progress + amount);
            return IsComplete;
        }

        public void MarkClaimed()
        {
            IsClaimed = true;
        }
    }
}
