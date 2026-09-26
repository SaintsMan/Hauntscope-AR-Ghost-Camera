using System.Collections.Generic;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.VirtualRoom
{
    // The virtual room never changes, so its hiding spots are read once from the markers the room was built with.
    public sealed class VirtualHideSpotProvider : IHideSpotProvider
    {
        private readonly List<Vector3> _spots = new List<Vector3>();

        public VirtualHideSpotProvider(Transform room)
        {
            foreach (var marker in room.GetComponentsInChildren<VirtualHideSpot>(true))
                _spots.Add(marker.transform.position);
        }

        public IReadOnlyList<Vector3> Spots => _spots;
    }
}
