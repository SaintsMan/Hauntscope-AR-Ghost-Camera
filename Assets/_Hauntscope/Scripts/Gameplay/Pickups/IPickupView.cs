using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    public interface IPickupView
    {
        void SetVisibility(float visibility);

        void SetPose(Vector3 position, float scale);

        void Despawn();
    }
}
