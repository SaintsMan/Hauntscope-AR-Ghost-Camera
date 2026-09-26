using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostSelector
    {
        private readonly GhostConfig _config;
        private readonly IRandom _random;
        private readonly WitchingHour _witchingHour;

        public GhostSelector(GhostConfig config, IRandom random, WitchingHour witchingHour)
        {
            _config = config;
            _random = random;
            _witchingHour = witchingHour;
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

            var night = _witchingHour.IsActive;
            var totalWeight = 0f;
            for (var i = 0; i < ghosts.Count; i++)
                totalWeight += WeightOf(ghosts[i], night);

            if (totalWeight <= 0f)
                return AnyAvailable(ghosts, night, _random.Range(0, ghosts.Count));

            // Each rarity's weight is shared by the ghosts of that rarity, so adding a Common ghost doesn't make Rare ones rarer.
            var roll = _random.Range(0f, totalWeight);
            for (var i = 0; i < ghosts.Count; i++)
            {
                roll -= WeightOf(ghosts[i], night);
                if (roll < 0f)
                    return ghosts[i];
            }

            return AnyAvailable(ghosts, night, ghosts.Count - 1);
        }

        // The first ghost that may come now, starting from the given index; a day with only night ghosts left falls
        // back to the list as it is.
        private static GhostData AnyAvailable(IReadOnlyList<GhostData> ghosts, bool night, int start)
        {
            for (var i = 0; i < ghosts.Count; i++)
            {
                var ghost = ghosts[(start + i) % ghosts.Count];
                if (IsAvailable(ghost, night))
                    return ghost;
            }

            return ghosts[start];
        }

        // Night-only ghosts are out of the pool by day, and a rarity's share goes to the ghosts that can come now.
        private float WeightOf(GhostData ghost, bool night)
        {
            if (!IsAvailable(ghost, night))
                return 0f;

            var sameRarity = 0;
            var ghosts = _config.Ghosts;
            for (var i = 0; i < ghosts.Count; i++)
            {
                if (ghosts[i].Rarity == ghost.Rarity && IsAvailable(ghosts[i], night))
                    sameRarity++;
            }

            var weight = _config.GetRarityWeight(ghost.Rarity) / sameRarity;
            return ghost.Rarity == GhostRarity.Legendary ? weight * _witchingHour.LegendaryWeightMultiplier : weight;
        }

        private static bool IsAvailable(GhostData ghost, bool night)
        {
            return night || !ghost.NightOnly;
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
