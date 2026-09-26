using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Hunt;

namespace Hauntscope.Gameplay.Contracts
{
    // Today's three agency contracts (GDD 5.29): a new board at local midnight (whatever was done but not claimed is
    // paid on the way), progress from each finished hunt, claiming, and replacing one for free or for a rewarded ad.
    public sealed class ContractBoard
    {
        private readonly EngagementProgress _engagement;
        private readonly EngagementRepository _repository;
        private readonly ContractGenerator _generator;
        private readonly RewardGranter _granter;
        private readonly IClock _clock;
        private readonly ContractConfig _config;
        private readonly IAdsService _ads;
        private readonly List<ContractSlot> _justCompleted = new List<ContractSlot>();
        private readonly List<ContractSlot> _fresh = new List<ContractSlot>();

        private bool _isReplacing;

        public ContractBoard(EngagementProgress engagement, EngagementRepository repository, ContractGenerator generator,
            RewardGranter granter, IClock clock, ContractConfig config, IAdsService ads)
        {
            _engagement = engagement;
            _repository = repository;
            _generator = generator;
            _granter = granter;
            _clock = clock;
            _config = config;
            _ads = ads;
        }

        public event Action Changed;

        // A finished hunt has been checked against the board; JustCompleted says what it completed.
        public event Action Recorded;

        public IReadOnlyList<ContractSlot> Slots => _engagement.Contracts;

        // Contracts the last recorded hunt completed, for the result card.
        public IReadOnlyList<ContractSlot> JustCompleted => _justCompleted;

        // yyyymmdd of the board, for the order numbers on the cards.
        public int Day => _engagement.ContractDay;

        public int ReadyCount
        {
            get
            {
                var count = 0;
                foreach (var slot in Slots)
                {
                    if (slot.IsReady)
                        count++;
                }

                return count;
            }
        }

        public bool HasFreeReplace => _engagement.FreeReplacesUsed < _config.FreeReplaces;

        public bool HasAdReplace => _engagement.AdReplacesUsed < _config.AdReplaces;

        public bool IsAdReady => _ads.IsRewardedReady;

        public bool IsReplacing => _isReplacing;

        // A clock set back never brings a board back: a new one comes only on a later day than the current board's.
        public void Refresh()
        {
            var today = LocalDay.Key(_clock.Today);
            if (today <= _engagement.ContractDay && Slots.Count > 0)
                return;

            foreach (var slot in Slots)
            {
                if (slot.IsReady)
                {
                    _granter.Grant(slot.Reward);
                    slot.MarkClaimed();
                }
            }

            _fresh.Clear();
            foreach (var tier in _config.Slots)
            {
                var slot = _generator.Generate(tier, _fresh);
                if (slot != null)
                    _fresh.Add(slot);
            }

            _engagement.StartContractDay(Math.Max(today, _engagement.ContractDay), _fresh);
            _justCompleted.Clear();
            Save();
        }

        public void Record(HuntReport report)
        {
            Refresh();
            _justCompleted.Clear();
            foreach (var slot in Slots)
            {
                if (slot.Advance(slot.Data.Goal.Count(report, slot.Subject)))
                    _justCompleted.Add(slot);
            }

            Save();
            Recorded?.Invoke();
        }

        // Returns the ectoplasm paid (gear beyond its limit included), or -1 when there was nothing to claim.
        public int Claim(int index)
        {
            if (index < 0 || index >= Slots.Count || !Slots[index].IsReady)
                return -1;

            var slot = Slots[index];
            var paid = _granter.Grant(slot.Reward);
            slot.MarkClaimed();
            Save();
            return paid;
        }

        public bool CanReplace(int index)
        {
            return !_isReplacing && IsOpen(index);
        }

        public bool ReplaceFree(int index)
        {
            if (_isReplacing || !HasFreeReplace || !Replace(index))
                return false;

            _engagement.UseFreeReplace();
            Save();
            return true;
        }

        public async UniTask<bool> ReplaceWithAdAsync(int index, CancellationToken cancellationToken)
        {
            if (!HasAdReplace || !_ads.IsRewardedReady || !CanReplace(index))
                return false;

            _isReplacing = true;
            Changed?.Invoke();
            try
            {
                if (!await _ads.ShowRewardedAsync(cancellationToken) || !Replace(index))
                    return false;

                _engagement.UseAdReplace();
                Save();
                return true;
            }
            finally
            {
                _isReplacing = false;
                Changed?.Invoke();
            }
        }

        private bool IsOpen(int index)
        {
            return index >= 0 && index < Slots.Count && !Slots[index].IsComplete;
        }

        private bool Replace(int index)
        {
            if (!IsOpen(index))
                return false;

            var next = _generator.Generate(Slots[index].Data.Tier, Slots);
            if (next == null)
                return false;

            _engagement.ReplaceContract(index, next);
            return true;
        }

        private void Save()
        {
            _repository.Save(_engagement);
            Changed?.Invoke();
        }
    }
}
