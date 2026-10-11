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
        private readonly RemoteTuning _tuning;

        public WitchingHour(IClock clock, NightConfig config, RemoteTuning tuning)
        {
            _clock = clock;
            _config = config;
            _tuning = tuning;
        }

        public bool IsActive
        {
            get
            {
                var hour = _clock.LocalNow.Hour;
                var start = _tuning.Int(RemoteKeys.WitchingHourStart, _config.StartHour, 0, RemoteKeys.LastHour);
                var end = _tuning.Int(RemoteKeys.WitchingHourEnd, _config.EndHour, 0, RemoteKeys.LastHour);
                // The night crosses midnight when it starts later than it ends.
                return start > end
                    ? hour >= start || hour < end
                    : hour >= start && hour < end;
            }
        }

        public float LegendaryWeightMultiplier => IsActive ? _config.LegendaryWeightMultiplier : 1f;

        // Applies to hunts that began in the night, whatever the clock says when the catch lands.
        public float NightRewardMultiplier => _config.RewardMultiplier;
    }
}
