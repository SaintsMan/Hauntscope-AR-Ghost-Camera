using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Iap
{
    // The rookie kit: offered once, after a couple of hunts, for a limited time that runs whether the game is open or not.
    public sealed class StarterOffer
    {
        private readonly IapConfig _config;
        private readonly PurchaseHistory _history;
        private readonly PurchaseHistoryRepository _repository;
        private readonly PlayerProgress _progress;
        private readonly IClock _clock;

        public StarterOffer(IapConfig config, PurchaseHistory history, PurchaseHistoryRepository repository, PlayerProgress progress,
            IClock clock)
        {
            _config = config;
            _history = history;
            _repository = repository;
            _progress = progress;
            _clock = clock;
        }

        public SupplyBundleData Product => _config.StarterPack;

        public bool IsActive => IsOpen && !IsBought && Remaining > TimeSpan.Zero;

        public TimeSpan Remaining
        {
            get
            {
                if (!IsOpen)
                    return TimeSpan.Zero;

                var left = EndsUtc - _clock.UtcNow;
                return left > TimeSpan.Zero ? left : TimeSpan.Zero;
            }
        }

        public DateTime EndsUtc => new DateTime(_history.StarterOfferStartTicks, DateTimeKind.Utc).AddHours(_config.StarterOfferHours);

        // The kit is pitched once, on the way back from a hunt: right after a ghost got away (the moment gear matters),
        // or a couple of hunts later if every ghost so far was caught.
        public bool ShouldAnnounce(bool ghostEscaped)
        {
            if (Product == null || _history.StarterOfferAnnounced || IsBought)
                return false;
            if (IsOpen && !IsActive)
                return false;

            var hunts = _progress.TotalSessions;
            return hunts >= _config.StarterOfferAfterHunts
                && (ghostEscaped || hunts >= _config.StarterOfferAfterHunts + _config.StarterOfferFallbackHunts);
        }

        private bool IsOpen => Product != null && _history.StarterOfferStartTicks != 0L;

        private bool IsBought => _history.Owns(Product.ProductId);

        // Starts the clock the first time the player qualifies.
        public void Refresh()
        {
            if (Product == null || IsOpen || _history.Owns(Product.ProductId))
                return;
            if (_progress.TotalSessions < _config.StarterOfferAfterHunts)
                return;

            _history.StartStarterOffer(_clock.UtcNow);
            _repository.Save(_history);
        }

        public void MarkAnnounced()
        {
            _history.MarkStarterOfferAnnounced();
            _repository.Save(_history);
        }
    }
}
