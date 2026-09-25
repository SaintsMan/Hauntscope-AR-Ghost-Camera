using Hauntscope.Gameplay.Ghosts;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeGhostView : IGhostView
    {
        public Vector3 Position { get; private set; }

        public int SetPositionCount { get; private set; }

        public void SetPosition(Vector3 position)
        {
            Position = position;
            SetPositionCount++;
        }
    }
}
