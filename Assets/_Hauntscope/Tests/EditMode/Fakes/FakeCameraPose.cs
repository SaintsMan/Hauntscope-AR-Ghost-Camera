using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeCameraPose : ICameraPose
    {
        public Vector3 Position { get; set; }

        public Vector3 Forward { get; set; } = Vector3.forward;

        public float Aspect { get; set; } = 9f / 16f;

        public Vector3 ViewportPoint { get; set; } = new Vector3(0.5f, 0.5f, 1f);

        // Optional real projection for tests that need points at different heights to land in different places.
        public System.Func<Vector3, Vector3> Projection { get; set; }

        public Vector3 WorldToViewport(Vector3 worldPosition)
        {
            return Projection != null ? Projection(worldPosition) : ViewportPoint;
        }
    }
}
