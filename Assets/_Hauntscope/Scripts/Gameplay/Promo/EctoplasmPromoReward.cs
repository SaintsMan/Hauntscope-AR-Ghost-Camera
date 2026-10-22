using Hauntscope.Gameplay.Engagement;

namespace Hauntscope.Gameplay.Promo
{
    public sealed class EctoplasmPromoReward : IPromoRewardHandler
    {
        private readonly RewardGranter _granter;

        public EctoplasmPromoReward(RewardGranter granter)
        {
            _granter = granter;
        }

        public string Type => PromoRewardTypes.Ectoplasm;

        public bool Grant(PromoReward reward)
        {
            if (reward.Amount <= 0)
                return false;

            _granter.Grant(new RewardBundle(reward.Amount));
            return true;
        }
    }
}
