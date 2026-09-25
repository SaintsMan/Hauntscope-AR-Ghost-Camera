using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostSelector
    {
        private readonly GhostConfig _config;
        private readonly IRandom _random;

        public GhostSelector(GhostConfig config, IRandom random)
        {
            _config = config;
            _random = random;
        }

        public GhostData Select(bool isFirstSession)
        {
            var ghosts = _config.Ghosts;
            if (ghosts.Count == 0)
                throw new InvalidOperationException("GhostConfig has no ghosts.");

            if (isFirstSession)
            {
                var first = FindById(_config.FirstGhostId);
                if (first != null)
                    return first;
            }

            var totalWeight = 0f;
            for (var i = 0; i < ghosts.Count; i++)
                totalWeight += WeightOf(ghosts[i]);

            if (totalWeight <= 0f)
                return ghosts[_random.Range(0, ghosts.Count)];

            // Each rarity's weight is shared by the ghosts of that rarity, so adding a Common ghost doesn't make Rare ones rarer.
            var roll = _random.Range(0f, totalWeight);
            for (var i = 0; i < ghosts.Count; i++)
            {
                roll -= WeightOf(ghosts[i]);
                if (roll < 0f)
                    return ghosts[i];
            }

            return ghosts[ghosts.Count - 1];
        }

        private float WeightOf(GhostData ghost)
        {
            var sameRarity = 0;
            var ghosts = _config.Ghosts;
            for (var i = 0; i < ghosts.Count; i++)
            {
                if (ghosts[i].Rarity == ghost.Rarity)
                    sameRarity++;
            }

            return _config.GetRarityWeight(ghost.Rarity) / sameRarity;
        }

        private GhostData FindById(string id)
        {
            var ghosts = _config.Ghosts;
            for (var i = 0; i < ghosts.Count; i++)
            {
                if (ghosts[i].Id == id)
                    return ghosts[i];
            }

            return null;
        }
    }
}
