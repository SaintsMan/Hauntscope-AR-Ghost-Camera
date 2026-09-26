using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Pickups
{
    public interface IPickupEffect
    {
        PickupGain Apply(HuntLoot loot, Battery battery);
    }
}
