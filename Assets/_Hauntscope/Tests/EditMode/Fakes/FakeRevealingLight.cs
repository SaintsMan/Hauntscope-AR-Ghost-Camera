using Hauntscope.Gameplay.Tools;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeRevealingLight : IRevealingLight
    {
        public bool IsLit { get; set; }

        public bool Lights(Vector3 position)
        {
            return IsLit;
        }
    }
}
