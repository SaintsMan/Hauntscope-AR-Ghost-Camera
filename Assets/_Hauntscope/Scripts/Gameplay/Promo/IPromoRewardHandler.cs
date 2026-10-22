namespace Hauntscope.Gameplay.Promo
{
    // Pays out one reward type from LiveOps/Promo/rewards.json; a new type is a new handler, nothing else changes.
    public interface IPromoRewardHandler
    {
        string Type { get; }

        // False when this reward cannot be paid (an unknown item id): the line is left out of what the player sees.
        bool Grant(PromoReward reward);
    }
}
