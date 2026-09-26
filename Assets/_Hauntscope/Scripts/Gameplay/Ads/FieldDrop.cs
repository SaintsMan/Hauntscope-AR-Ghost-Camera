using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Ads
{
    // "Field drop" in the supply depot: a rewarded ad for ectoplasm, a few times per local day.
    public sealed class FieldDrop : IDisposable
    {
        private readonly IAdsService _ads;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly IClock _clock;
        private readonly AdsConfig _config;

        private bool _isClaiming;

        public FieldDrop(IAdsService ads, PlayerProgress progress, PlayerProgressRepository repository, IClock clock, AdsConfig config)
        {
            _ads = ads;
            _progress = progress;
            _repository = repository;
            _clock = clock;
            _config = config;
            _ads.AvailabilityChanged += OnAvailabilityChanged;
        }

        public event Action Changed;

        public int Reward => _config.FieldDropReward;

        public int PerDay => _config.FieldDropsPerDay;

        public int RemainingToday => Math.Max(0, _config.FieldDropsPerDay - _progress.FieldDropsClaimedOn(Today));

        public bool IsReady => _ads.IsRewardedReady;

        public bool CanClaim => !_isClaiming && RemainingToday > 0 && _ads.IsRewardedReady;

        private int Today => DayKey(_clock.Today);

        public async UniTask<bool> ClaimAsync(CancellationToken cancellationToken)
        {
            if (!CanClaim)
                return false;

            _isClaiming = true;
            Changed?.Invoke();
            try
            {
                if (!await _ads.ShowRewardedAsync(cancellationToken))
                    return false;

                _progress.ClaimFieldDrop(Today, _config.FieldDropReward);
                _repository.Save(_progress);
                return true;
            }
            finally
            {
                _isClaiming = false;
                Changed?.Invoke();
            }
        }

        public void Dispose()
        {
            _ads.AvailabilityChanged -= OnAvailabilityChanged;
        }

        public static int DayKey(DateTime date)
        {
            return LocalDay.Key(date);
        }

        private void OnAvailabilityChanged()
        {
            Changed?.Invoke();
        }
    }
}
