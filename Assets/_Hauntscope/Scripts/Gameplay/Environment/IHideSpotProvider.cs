using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Environment
{
    // Floor points where a ghost can sit out of sight: behind furniture in the virtual room, along walls in AR.
    public interface IHideSpotProvider
    {
        IReadOnlyList<Vector3> Spots { get; }
    }
}
