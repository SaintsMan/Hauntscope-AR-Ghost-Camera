using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Pickups
{
    public sealed class EctoplasmPickupEffect : IPickupEffect
    {
        private readonly int _amount;

        public EctoplasmPickupEffect(int amount)
        {
            _amount = amount;
        }

        public PickupGain Apply(HuntLoot loot, Battery battery)
        {
            loot.Add(_amount);
            return new PickupGain(_amount, 0f);
        }
    }
}
