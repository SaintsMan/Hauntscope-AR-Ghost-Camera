using System;
using Hauntscope.Gameplay.Tools;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeLensSight : ILensSight
    {
        public Func<Vector3, bool> Looking { get; set; } = _ => false;

        public bool Sees(Vector3 position, float range)
        {
            return Looking(position);
        }
    }
}
