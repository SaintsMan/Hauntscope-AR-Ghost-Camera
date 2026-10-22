using System;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Season
{
    // The candy purse and the skin cabinet of the Halloween event (GDD 5.42). Candy only comes and goes while the event
    // runs and burns the day after it ends; a bought skin is the player's for good, and can be worn any time.
    public sealed class HalloweenCollection : IStartable, IDisposable
    {
        private readonly HalloweenProgress _progress;
        private readonly HalloweenRepository _repository;
        private readonly SeasonEvent _season;
        private readonly HalloweenConfig _config;
        private readonly IClock _clock;
        private readonly DemoSwitches _demo;
        private readonly IRemoteValues _remote;
        private readonly ObservableValue<int> _candy = new ObservableValue<int>();

        public HalloweenCollection(HalloweenProgress progress, HalloweenRepository repository, SeasonEvent season, HalloweenConfig config,
            IClock clock, DemoSwitches demo, IRemoteValues remote)
        {
            _remote = remote;
            _progress = progress;
            _repository = repository;
            _season = season;
            _config = config;
            _clock = clock;
            _demo = demo;
        }

        public event Action<int> CandyAdded;

        public event Action<HalloweenSkin> SkinUnlocked;

        // A skin was put on or taken off.
        public event Action Worn;

        public IReadOnlyObservableValue<int> Candy => _candy;

        public bool IsRunning => _season.IsHalloween;

        public int DaysLeft => _season.HalloweenDaysLeft;

        public HalloweenSkin[] Skins => _config.Skins;

        public void Start()
        {
            if (!_progress.IsBurned && _clock.Today.Date > _season.HalloweenEnd.Date)
            {
                _progress.Burn();
                _repository.Save(_progress);
            }

            if (_demo.Candy > _progress.Candy && !_progress.IsBurned)
                _progress.SetCandy(_demo.Candy);

            _candy.Value = _progress.Candy;
            Welcome();
            // On a fresh install the event is only switched on by the first Remote Config fetch, after this start.
            _remote.Applied += Welcome;
        }

        public void Dispose()
        {
            _remote.Applied -= Welcome;
        }

        public void Add(int amount)
        {
            if (amount <= 0 || !IsRunning)
                return;

            _progress.AddCandy(amount);
            _candy.Value = _progress.Candy;
            _repository.Save(_progress);
            CandyAdded?.Invoke(amount);
        }

        public bool Owns(HalloweenSkin skin)
        {
            return _progress.Owns(skin.Id);
        }

        public int PriceOf(HalloweenSkin skin)
        {
            return _season.SkinPrice(skin);
        }

        public SkinPurchase Buy(HalloweenSkin skin)
        {
            if (Owns(skin))
                return SkinPurchase.AlreadyOwned;
            if (!IsRunning)
                return SkinPurchase.EventOver;

            var price = PriceOf(skin);
            if (_progress.Candy < price)
                return SkinPurchase.NotEnoughCandy;

            _progress.Unlock(skin.Id, price);
            _progress.Wear(skin.Slot, skin.Id);
            _candy.Value = _progress.Candy;
            _repository.Save(_progress);
            SkinUnlocked?.Invoke(skin);
            Worn?.Invoke();
            return SkinPurchase.Bought;
        }

        private void Welcome()
        {
            if (!IsRunning || _progress.IsWelcomed || _progress.IsBurned)
                return;

            _progress.Welcome(_season.HalloweenWelcomeCandy);
            _candy.Value = _progress.Candy;
            _repository.Save(_progress);
        }

        public bool IsWorn(HalloweenSkin skin)
        {
            return _progress.WornIn(skin.Slot) == skin.Id;
        }

        // Wears an owned skin, or takes it off again when it is already worn.
        public void Toggle(HalloweenSkin skin)
        {
            if (!Owns(skin))
                return;

            _progress.Wear(skin.Slot, IsWorn(skin) ? string.Empty : skin.Id);
            _repository.Save(_progress);
            Worn?.Invoke();
        }

        // The skin worn in the slot, or null for the plain kit.
        public HalloweenSkin WornIn(HalloweenSkinSlot slot)
        {
            var id = _progress.WornIn(slot);
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (var skin in _config.Skins)
            {
                if (skin.Id == id)
                    return skin;
            }

            return null;
        }
    }
}
