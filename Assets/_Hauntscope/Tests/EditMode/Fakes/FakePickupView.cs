using Hauntscope.Gameplay.Pickups;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePickupView : IPickupView
    {
        public float Visibility { get; private set; }

        public Vector3 Position { get; private set; }

        public float Scale { get; private set; } = 1f;

        public bool IsDespawned { get; private set; }

        public void SetVisibility(float visibility)
        {
            Visibility = visibility;
        }

        public void SetPose(Vector3 position, float scale)
        {
            Position = position;
            Scale = scale;
        }

        public void Despawn()
        {
            IsDespawned = true;
        }
    }
}
