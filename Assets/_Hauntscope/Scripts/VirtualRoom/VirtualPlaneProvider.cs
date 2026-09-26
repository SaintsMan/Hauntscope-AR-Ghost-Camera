using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.VirtualRoom
{
    public sealed class VirtualPlaneProvider : IPlaneProvider
    {
        // Casting down from below table height: the first thing hit is the floor only where nothing stands on it.
        private const float ProbeHeight = 1.2f;

        private readonly Collider _floor;

        public VirtualPlaneProvider(Collider floor, RoomConfig config)
        {
            _floor = floor;
            var floorBounds = floor.bounds;
            FloorHeight = floorBounds.max.y;

            // AR planes cover less than the real room, so AR pads outwards; here the floor is exact and the walls
            // are real geometry, so the padding keeps ghosts inside them instead.
            var inset = config.RoomBoundsPadding * 2f;
            var size = new Vector3(
                Mathf.Max(0f, floorBounds.size.x - inset),
                0f,
                Mathf.Max(0f, floorBounds.size.z - inset));
            RoomBounds = new Bounds(new Vector3(floorBounds.center.x, FloorHeight, floorBounds.center.z), size);
            HorizontalArea = floorBounds.size.x * floorBounds.size.z;
        }

        public float HorizontalArea { get; }

        public Bounds RoomBounds { get; }

        public float FloorHeight { get; }

        public void SetPlanesVisible(bool visible)
        {
        }

        public bool IsFloorPoint(Vector3 point)
        {
            var origin = new Vector3(point.x, FloorHeight + ProbeHeight, point.z);
            return Physics.Raycast(origin, Vector3.down, out var hit, ProbeHeight * 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                && hit.collider == _floor;
        }
    }
}
