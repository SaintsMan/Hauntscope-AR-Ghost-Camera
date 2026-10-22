namespace Hauntscope.Gameplay.Promo
{
    // One line of a code's payout as the server sends it: a type from LiveOps/Promo/rewards.json, how many, and the
    // item id for types that carry one (gear).
    public sealed class PromoReward
    {
        public PromoReward(string type, int amount, string id = null)
        {
            Type = type;
            Amount = amount;
            Id = id;
        }

        public string Type { get; }

        public int Amount { get; }

        public string Id { get; }
    }
}
