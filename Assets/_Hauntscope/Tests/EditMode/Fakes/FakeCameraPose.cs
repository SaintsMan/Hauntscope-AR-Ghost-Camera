using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeCameraPose : ICameraPose
    {
        public Vector3 Position { get; set; }

        public Vector3 Forward { get; set; } = Vector3.forward;

        public Vector3 WorldToViewport(Vector3 worldPosition)
        {
            return worldPosition;
        }
    }
}
