using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Pickups
{
    // A piece of the event's candy: it goes into the hunt's loot and reaches the purse with the result card.
    public sealed class CandyPickupEffect : IPickupEffect
    {
        private readonly int _amount;

        public CandyPickupEffect(int amount)
        {
            _amount = amount;
        }

        public PickupGain Apply(HuntLoot loot, Battery battery)
        {
            loot.AddCandy(_amount);
            return new PickupGain(0, 0f, _amount);
        }
    }
}
