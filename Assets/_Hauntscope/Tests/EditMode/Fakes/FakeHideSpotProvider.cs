using System.Collections.Generic;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeHideSpotProvider : IHideSpotProvider
    {
        public List<Vector3> SpotList { get; } = new List<Vector3>();

        public IReadOnlyList<Vector3> Spots => SpotList;
    }
}
