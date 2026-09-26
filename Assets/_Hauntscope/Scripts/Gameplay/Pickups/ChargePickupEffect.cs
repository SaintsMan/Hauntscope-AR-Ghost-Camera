using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Pickups
{
    public sealed class ChargePickupEffect : IPickupEffect
    {
        private readonly float _charge;

        public ChargePickupEffect(float charge)
        {
            _charge = charge;
        }

        public PickupGain Apply(HuntLoot loot, Battery battery)
        {
            battery.Recharge(_charge);
            return new PickupGain(0, _charge);
        }
    }
}
