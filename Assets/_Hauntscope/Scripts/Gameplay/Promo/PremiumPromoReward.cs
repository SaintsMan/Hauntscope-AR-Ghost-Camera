using Hauntscope.Gameplay.Iap;

namespace Hauntscope.Gameplay.Promo
{
    // The full version for good: no ads and the premium perks, as if bought.
    public sealed class PremiumPromoReward : IPromoRewardHandler
    {
        private readonly IapGrant _grant;

        public PremiumPromoReward(IapGrant grant)
        {
            _grant = grant;
        }

        public string Type => PromoRewardTypes.Premium;

        public bool Grant(PromoReward reward)
        {
            _grant.UnlockPremium();
            return true;
        }
    }
}
