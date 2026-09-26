using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Ghosts
{
    // GDD 5.28: from 23:00 to 04:00 on the player's clock the night-only ghosts come out, legendaries turn up more
    // often and every catch pays more.
    public sealed class WitchingHour
    {
        private readonly IClock _clock;
        private readonly NightConfig _config;

        public WitchingHour(IClock clock, NightConfig config)
        {
            _clock = clock;
            _config = config;
        }

        public bool IsActive
        {
            get
            {
                var hour = _clock.LocalNow.Hour;
                // The night crosses midnight when it starts later than it ends.
                return _config.StartHour > _config.EndHour
                    ? hour >= _config.StartHour || hour < _config.EndHour
                    : hour >= _config.StartHour && hour < _config.EndHour;
            }
        }

        public float LegendaryWeightMultiplier => IsActive ? _config.LegendaryWeightMultiplier : 1f;

        // Applies to hunts that began in the night, whatever the clock says when the catch lands.
        public float NightRewardMultiplier => _config.RewardMultiplier;
    }
}
