using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Iap
{
    // Whether the full version may be pitched now: to a regular player (they have met the ads by then), every few days.
    // OfferFunnel picks the moment: the third catch, or right after an ad break.
    public sealed class PremiumOffer
    {
        private readonly IapConfig _config;
        private readonly PurchaseHistory _history;
        private readonly PurchaseHistoryRepository _repository;
        private readonly PlayerProgress _progress;
        private readonly IClock _clock;
        private readonly RemoteTuning _tuning;

        public PremiumOffer(IapConfig config, PurchaseHistory history, PurchaseHistoryRepository repository, PlayerProgress progress,
            IClock clock, RemoteTuning tuning)
        {
            _config = config;
            _history = history;
            _repository = repository;
            _progress = progress;
            _clock = clock;
            _tuning = tuning;
        }

        public FullVersionData Product => _config.FullVersion;

        public bool ShouldShow
        {
            get
            {
                if (Product == null || _history.IsPremium)
                    return false;
                if (_progress.TotalSessions < _tuning.Int(RemoteKeys.PremiumOfferAfterHunts, _config.PremiumOfferAfterHunts, 0, RemoteKeys.MaxHunts))
                    return false;

                return IsDue;
            }
        }

        // The third catch (premium_offer_at_captures): the agent knows by now the game is worth it, and the pitch comes
        // whatever the hunt count, unless it was shown within the last few days. 0 turns the moment off.
        public bool IsCatchMoment
        {
            get
            {
                if (Product == null || _history.IsPremium)
                    return false;

                var captures = _tuning.Int(RemoteKeys.PremiumOfferAtCaptures, _config.PremiumOfferAtCaptures, 0, RemoteKeys.MaxHunts);
                return captures > 0 && _progress.TotalCaptures == captures && IsDue;
            }
        }

        private bool IsDue
        {
            get
            {
                var everyDays = _tuning.Int(RemoteKeys.PremiumOfferEveryDays, _config.PremiumOfferEveryDays, 1, RemoteKeys.MaxDays);
                return _history.PremiumOfferShownDay == 0 || Today - _history.PremiumOfferShownDay >= everyDays;
            }
        }

        private int Today => (int)(_clock.Today.Ticks / TimeSpan.TicksPerDay);

        public void MarkShown()
        {
            _history.MarkPremiumOfferShown(Today);
            _repository.Save(_history);
        }
    }
}
