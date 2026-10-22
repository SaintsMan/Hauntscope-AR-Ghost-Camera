using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Season;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostSelector
    {
        private readonly GhostConfig _config;
        private readonly IRandom _random;
        private readonly WitchingHour _witchingHour;
        private readonly SeasonEvent _season;
        private readonly DemoSwitches _demo;

        // Tests of the ordinary draw: no season runs and nothing is forced.
        public GhostSelector(GhostConfig config, IRandom random, WitchingHour witchingHour)
            : this(config, random, witchingHour, null, DemoSwitches.Off)
        {
        }

        public GhostSelector(GhostConfig config, IRandom random, WitchingHour witchingHour, SeasonEvent season, DemoSwitches demo)
        {
            _config = config;
            _random = random;
            _witchingHour = witchingHour;
            _season = season;
            _demo = demo;
        }

        // The first two hunts are set (the tutorial ghost, then the one that teaches vulnerability); after that night
        // shift rounds bring their own rarity split, otherwise the config's applies.
        public GhostData Select(int huntsPlayed, RarityWeights weights = null)
        {
            var ghosts = _config.Ghosts;
            if (ghosts.Count == 0)
                throw new InvalidOperationException("GhostConfig has no ghosts.");

            var forced = FindById(_demo.Ghost);
            if (forced != null)
                return forced;

            var first = huntsPlayed == 0 ? FindById(_config.FirstGhostId) : null;
            if (first != null)
                return first;

            // A season's ghost may take the second hunt's place: the event is short and new agents play few hunts.
            var seasonal = PickSeasonal(ghosts, huntsPlayed + 1);
            if (seasonal != null)
                return seasonal;

            var second = huntsPlayed == 1 ? FindById(_config.SecondGhostId) : null;
            if (second != null)
                return second;

            var night = _witchingHour.IsActive;
            var totalWeight = 0f;
            for (var i = 0; i < ghosts.Count; i++)
                totalWeight += WeightOf(ghosts[i], night, weights);

            if (totalWeight <= 0f)
                return AnyAvailable(ghosts, night, _random.Range(0, ghosts.Count));

            // Each rarity's weight is shared by the ghosts of that rarity, so adding a Common ghost doesn't make Rare ones rarer.
            var roll = _random.Range(0f, totalWeight);
            for (var i = 0; i < ghosts.Count; i++)
            {
                roll -= WeightOf(ghosts[i], night, weights);
                if (roll < 0f)
                    return ghosts[i];
            }

            return AnyAvailable(ghosts, night, ghosts.Count - 1);
        }

        // A season's ghost comes by its own chance, on top of the ordinary draw, so the console's percent is what it means.
        private GhostData PickSeasonal(IReadOnlyList<GhostData> ghosts, int huntNumber)
        {
            if (_season == null)
                return null;

            for (var i = 0; i < ghosts.Count; i++)
            {
                var ghost = ghosts[i];
                if (string.IsNullOrEmpty(ghost.SeasonEvent))
                    continue;

                if (huntNumber < _season.GhostFromHunt(ghost.SeasonEvent))
                    continue;

                var percent = _season.GhostPercent(ghost.SeasonEvent);
                if (percent > 0 && _random.Range(0f, 100f) < percent)
                    return ghost;
            }

            return null;
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
        private float WeightOf(GhostData ghost, bool night, RarityWeights weights)
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

            var share = weights != null ? weights.WeightOf(ghost.Rarity) : _config.GetRarityWeight(ghost.Rarity);
            var weight = share / sameRarity;
            return ghost.Rarity == GhostRarity.Legendary ? weight * _witchingHour.LegendaryWeightMultiplier : weight;
        }

        private static bool IsAvailable(GhostData ghost, bool night)
        {
            return string.IsNullOrEmpty(ghost.SeasonEvent) && (night || !ghost.NightOnly);
        }

        private GhostData FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

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
