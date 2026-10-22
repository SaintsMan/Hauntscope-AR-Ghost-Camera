using System.Collections.Generic;
using Hauntscope.Gameplay.Promo;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePromoRewardHandler : IPromoRewardHandler
    {
        public FakePromoRewardHandler(string type)
        {
            Type = type;
        }

        public string Type { get; }

        public List<PromoReward> Granted { get; } = new List<PromoReward>();

        public bool Grant(PromoReward reward)
        {
            Granted.Add(reward);
            return true;
        }
    }
}
