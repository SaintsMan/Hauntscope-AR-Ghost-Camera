using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Engagement
{
    // The daily ration (GDD 5.30): one frame of the cassette per local day the game is opened. A missed day costs
    // nothing, the cassette only moves on days with a claim, and a clock set back never pays twice.
    public sealed class LoginCalendar : IDisposable
    {
        private readonly EngagementProgress _engagement;
        private readonly EngagementRepository _repository;
        private readonly LoginConfig _config;
        private readonly RewardGranter _granter;
        private readonly IClock _clock;
        private readonly IAdsService _ads;

        private bool _isClaiming;

        public LoginCalendar(EngagementProgress engagement, EngagementRepository repository, LoginConfig config, RewardGranter granter,
            IClock clock, IAdsService ads)
        {
            _engagement = engagement;
            _repository = repository;
            _config = config;
            _granter = granter;
            _clock = clock;
            _ads = ads;
            _ads.AvailabilityChanged += OnAvailabilityChanged;
        }

        public event Action Changed;

        public int Length => _config.Days.Count;

        public bool CanClaim => Length > 0 && !_isClaiming && Today > _engagement.LastLoginDay;

        public bool IsClaimedToday => Length > 0 && Today <= _engagement.LastLoginDay;

        // The frame for today: the one to claim, or the one already claimed today.
        public int TodayIndex
        {
            get
            {
                if (Length == 0)
                    return 0;

                var claims = IsClaimedToday ? _engagement.LoginClaims - 1 : _engagement.LoginClaims;
                return Math.Max(0, claims) % Length;
            }
        }

        public LoginReward Reward(int index) => _config.Days[index];

        public LoginReward TodayReward => Length > 0 ? _config.Days[TodayIndex] : null;

        public int AdMultiplier => _config.AdMultiplier;

        // The ad doubles ectoplasm only, so a day of gear alone offers no ad.
        public bool IsDoubleOffered => CanClaim && TodayReward.Ectoplasm > 0 && _config.AdMultiplier > 1;

        public bool CanDouble => IsDoubleOffered && _ads.IsRewardedReady;

        // Opens by itself on the first menu of a day with something to claim; after that only from its icon.
        public bool ShouldOpenByItself => CanClaim && Today > _engagement.LastLoginShownDay;

        private int Today => LocalDay.Key(_clock.Today);

        public void MarkShown()
        {
            _engagement.MarkLoginShown(Today);
            _repository.Save(_engagement);
        }

        // Null when there was nothing to claim.
        public RewardBundle Claim()
        {
            return CanClaim ? Grant(TodayReward.ToBundle()) : null;
        }

        public async UniTask<RewardBundle> ClaimDoubledAsync(CancellationToken cancellationToken)
        {
            if (!CanDouble)
                return null;

            _isClaiming = true;
            Changed?.Invoke();
            try
            {
                if (!await _ads.ShowRewardedAsync(cancellationToken))
                    return null;
            }
            finally
            {
                _isClaiming = false;
            }

            return Grant(TodayReward.ToBundle().WithEctoplasmTimes(_config.AdMultiplier));
        }

        private RewardBundle Grant(RewardBundle bundle)
        {
            _granter.Grant(bundle);
            _engagement.ClaimLogin(Today);
            _repository.Save(_engagement);
            Changed?.Invoke();
            return bundle;
        }

        public void Dispose()
        {
            _ads.AvailabilityChanged -= OnAvailabilityChanged;
        }

        private void OnAvailabilityChanged()
        {
            Changed?.Invoke();
        }
    }
}
