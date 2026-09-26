using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;

namespace Hauntscope.Gameplay.Contracts
{
    // Draws a contract of a tier that is not on the board yet and that the player can already do, and sets its pay.
    public sealed class ContractGenerator
    {
        private readonly ContractConfig _config;
        private readonly StoreConfig _store;
        private readonly IRandom _random;
        private readonly ContractContext _context;
        private readonly List<ContractData> _candidates = new List<ContractData>();
        private readonly List<string> _subjects = new List<string>();

        public ContractGenerator(ContractConfig config, StoreConfig store, GhostConfig ghosts, ShiftConfig shift, PlayerProgress progress,
            IRandom random)
        {
            _config = config;
            _store = store;
            _random = random;
            _context = new ContractContext(progress, ghosts.Ghosts, shift, random);
        }

        // Null when no contract of the tier can be offered.
        public ContractSlot Generate(ContractTier tier, IReadOnlyList<ContractSlot> board)
        {
            _candidates.Clear();
            _subjects.Clear();
            foreach (var data in _config.Contracts)
            {
                if (data == null || data.Tier != tier || data.Goal == null || IsOnBoard(data, board))
                    continue;
                if (!data.Goal.TrySelectSubject(_context, out var subject))
                    continue;

                _candidates.Add(data);
                _subjects.Add(subject);
            }

            if (_candidates.Count == 0)
                return null;

            var index = _random.Range(0, _candidates.Count);
            return new ContractSlot(_candidates[index], _subjects[index], Reward(tier));
        }

        // Some hard contracts pay in gear instead of ectoplasm.
        private RewardBundle Reward(ContractTier tier)
        {
            var boosters = _store.Boosters;
            if (boosters.Count > 0 && _random.Value < _config.GearChance(tier))
                return new RewardBundle(0, boosters[_random.Range(0, boosters.Count)], 1);
            return new RewardBundle(_config.Reward(tier));
        }

        private static bool IsOnBoard(ContractData data, IReadOnlyList<ContractSlot> board)
        {
            foreach (var slot in board)
            {
                if (slot != null && slot.Data.Id == data.Id)
                    return true;
            }

            return false;
        }
    }
}
