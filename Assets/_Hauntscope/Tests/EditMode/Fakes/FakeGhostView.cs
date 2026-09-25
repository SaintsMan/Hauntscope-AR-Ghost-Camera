using Hauntscope.Gameplay.Ghosts;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeGhostView : IGhostView
    {
        public Vector3 Position { get; private set; }

        public Quaternion Rotation { get; private set; }

        public float Reveal { get; private set; }

        public int SetPoseCount { get; private set; }

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
            SetPoseCount++;
        }

        public void SetReveal(float reveal)
        {
            Reveal = reveal;
        }
    }
}
