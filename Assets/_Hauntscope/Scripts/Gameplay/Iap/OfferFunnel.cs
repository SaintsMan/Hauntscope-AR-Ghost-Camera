using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Store;

namespace Hauntscope.Gameplay.Iap
{
    // Which paid offer, if any, the menu shows on the way back from a hunt (GDD 5.37). Each one answers a moment the
    // player has just lived through: the rookie kit after a ghost got away (or a few hunts in), the full version on
    // the third catch and right after an ad break, the field kit after the battery died with no spare left. Never on a cold start, at most one.
    public sealed class OfferFunnel
    {
        private readonly OfferMoments _moments;
        private readonly StarterOffer _starter;
        private readonly PremiumOffer _premium;
        private readonly IapConfig _config;
        private readonly PurchaseHistory _history;
        private readonly PurchaseHistoryRepository _repository;
        private readonly PlayerInventory _inventory;
        private readonly StoreConfig _store;
        private readonly IClock _clock;
        private readonly RemoteTuning _tuning;
        private readonly IIapStore _iap;

        public OfferFunnel(OfferMoments moments, StarterOffer starter, PremiumOffer premium, IapConfig config, PurchaseHistory history,
            PurchaseHistoryRepository repository, PlayerInventory inventory, StoreConfig store, IClock clock, RemoteTuning tuning,
            IIapStore iap)
        {
            _iap = iap;
            _tuning = tuning;
            _moments = moments;
            _starter = starter;
            _premium = premium;
            _config = config;
            _history = history;
            _repository = repository;
            _inventory = inventory;
            _store = store;
            _clock = clock;
        }

        // Answers the moments once: a second call before the next hunt returns null.
        public IapProductData Pick()
        {
            if (!_moments.HuntEnded)
                return null;

            var escaped = _moments.LastHuntEscaped;
            var adBreak = _moments.SawAdBreak;
            var captured = _moments.LastHuntCaptured;
            _moments.Clear();
            if (!_tuning.Bool(RemoteKeys.OffersEnabled, _config.OffersEnabled))
                return null;
            // Without prices the card is a dead button, and the rookie kit is only ever pitched once: wait for the store.
            if (!_iap.IsReady)
                return null;

            // The catch that proves the game is worth it outranks the rookie kit: it happens once.
            if (captured && _premium.IsCatchMoment)
            {
                _premium.MarkShown();
                return _premium.Product;
            }

            if (_starter.ShouldAnnounce(escaped))
            {
                _starter.Refresh();
                _starter.MarkAnnounced();
                return _starter.Product;
            }

            if (adBreak && _premium.ShouldShow)
            {
                _premium.MarkShown();
                return _premium.Product;
            }

            if (escaped && ShouldOfferKit())
            {
                _history.MarkKitOfferShown(Today);
                _repository.Save(_history);
                return _config.KitOffer;
            }

            return null;
        }

        private bool ShouldOfferKit()
        {
            if (_config.KitOffer == null || _store.SpareBattery == null || _inventory.GetCount(_store.SpareBattery.Id) > 0)
                return false;

            var everyDays = _tuning.Int(RemoteKeys.KitOfferEveryDays, _config.KitOfferEveryDays, 1, RemoteKeys.MaxDays);
            return _history.KitOfferShownDay == 0 || Today - _history.KitOfferShownDay >= everyDays;
        }

        private int Today => (int)(_clock.Today.Ticks / System.TimeSpan.TicksPerDay);
    }
}
