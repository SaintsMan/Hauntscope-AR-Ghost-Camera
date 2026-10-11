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
        private readonly RemoteTuning _tuning;

        public InterstitialPolicy(AdPacing pacing, PlayerProgress progress, IClock clock, AdsConfig config, RemoteTuning tuning)
        {
            _pacing = pacing;
            _progress = progress;
            _clock = clock;
            _config = config;
            _tuning = tuning;
        }

        public bool ShouldShow()
        {
            if (!_tuning.Bool(RemoteKeys.InterstitialEnabled, _config.InterstitialEnabled))
                return false;
            if (_progress.TotalSessions < _tuning.Int(RemoteKeys.InterstitialMinSessions, _config.InterstitialMinSessions, 0, RemoteKeys.MaxHunts))
                return false;
            if (_pacing.HuntsSinceInterstitial < _tuning.Int(RemoteKeys.InterstitialEveryHunts, _config.InterstitialEveryHunts, 1, RemoteKeys.MaxHunts))
                return false;

            var minInterval = _tuning.Int(RemoteKeys.InterstitialMinIntervalSeconds, (int)_config.InterstitialMinInterval, 0, RemoteKeys.MaxSeconds);
            var last = _pacing.LastAdAt;
            return !last.HasValue || (_clock.UtcNow - last.Value).TotalSeconds >= minInterval;
        }
    }
}
