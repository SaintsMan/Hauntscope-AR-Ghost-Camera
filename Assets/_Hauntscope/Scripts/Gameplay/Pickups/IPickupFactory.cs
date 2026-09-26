using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    public interface IPickupFactory
    {
        Pickup Create(PickupData data, Vector3 position);
    }
}
