using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Iap
{
    // When the menu reminds a regular player that the full version exists: after a few hunts (they have met the ads by
    // then), every few days, and never on the day the rookie kit is announced.
    public sealed class PremiumOffer
    {
        private readonly IapConfig _config;
        private readonly PurchaseHistory _history;
        private readonly PurchaseHistoryRepository _repository;
        private readonly PlayerProgress _progress;
        private readonly StarterOffer _starter;
        private readonly IClock _clock;

        public PremiumOffer(IapConfig config, PurchaseHistory history, PurchaseHistoryRepository repository, PlayerProgress progress,
            StarterOffer starter, IClock clock)
        {
            _config = config;
            _history = history;
            _repository = repository;
            _progress = progress;
            _starter = starter;
            _clock = clock;
        }

        public FullVersionData Product => _config.FullVersion;

        public bool ShouldShow
        {
            get
            {
                if (Product == null || _history.IsPremium || _starter.ShouldAnnounce)
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
