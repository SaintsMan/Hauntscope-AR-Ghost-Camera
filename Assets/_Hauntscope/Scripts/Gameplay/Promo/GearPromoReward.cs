using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;

namespace Hauntscope.Gameplay.Promo
{
    // Boosters and spare batteries by their store id; beyond the stack limit they turn into ectoplasm, like any reward.
    public sealed class GearPromoReward : IPromoRewardHandler
    {
        private readonly RewardGranter _granter;
        private readonly StoreConfig _store;

        public GearPromoReward(RewardGranter granter, StoreConfig store)
        {
            _granter = granter;
            _store = store;
        }

        public string Type => PromoRewardTypes.Gear;

        public bool Grant(PromoReward reward)
        {
            var gear = _store.FindGear(reward.Id);
            if (gear == null || reward.Amount <= 0)
                return false;

            _granter.Grant(new RewardBundle(0, gear, reward.Amount));
            return true;
        }
    }
}
