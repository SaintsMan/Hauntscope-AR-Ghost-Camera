using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Iap
{
    // Whether the full version may be pitched now: to a regular player (they have met the ads by then), every few days.
    // OfferFunnel picks the moment: right after an ad break.
    public sealed class PremiumOffer
    {
        private readonly IapConfig _config;
        private readonly PurchaseHistory _history;
        private readonly PurchaseHistoryRepository _repository;
        private readonly PlayerProgress _progress;
        private readonly IClock _clock;

        public PremiumOffer(IapConfig config, PurchaseHistory history, PurchaseHistoryRepository repository, PlayerProgress progress,
            IClock clock)
        {
            _config = config;
            _history = history;
            _repository = repository;
            _progress = progress;
            _clock = clock;
        }

        public FullVersionData Product => _config.FullVersion;

        public bool ShouldShow
        {
            get
            {
                if (Product == null || _history.IsPremium)
                    return false;
                if (_progress.TotalSessions < _config.PremiumOfferAfterHunts)
                    return false;

                return _history.PremiumOfferShownDay == 0 || Today - _history.PremiumOfferShownDay >= _config.PremiumOfferEveryDays;
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
