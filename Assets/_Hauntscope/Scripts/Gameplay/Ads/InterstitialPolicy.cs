using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Ads
{
    // GDD 5.23: an interstitial only between hunts, never for a new player, at most every few hunts, and never
    // right after another ad.
    public sealed class InterstitialPolicy
    {
        private readonly AdPacing _pacing;
        private readonly PlayerProgress _progress;
        private readonly IClock _clock;
        private readonly AdsConfig _config;

        public InterstitialPolicy(AdPacing pacing, PlayerProgress progress, IClock clock, AdsConfig config)
        {
            _pacing = pacing;
            _progress = progress;
            _clock = clock;
            _config = config;
        }

        public bool ShouldShow()
        {
            if (_progress.TotalSessions < _config.InterstitialMinSessions)
                return false;
            if (_pacing.HuntsSinceInterstitial < _config.InterstitialEveryHunts)
                return false;

            var last = _pacing.LastAdAt;
            return !last.HasValue || (_clock.UtcNow - last.Value).TotalSeconds >= _config.InterstitialMinInterval;
        }
    }
}
