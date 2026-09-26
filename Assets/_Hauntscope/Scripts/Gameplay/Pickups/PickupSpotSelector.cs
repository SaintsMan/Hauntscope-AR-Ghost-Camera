using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    // A free patch of real floor away from the player and from other pickups, so they are spread around the room.
    public sealed class PickupSpotSelector
    {
        private const int MaxAttempts = 40;

        private readonly IPlaneProvider _planes;
        private readonly IRandom _random;
        private readonly PickupConfig _config;

        public PickupSpotSelector(IPlaneProvider planes, IRandom random, PickupConfig config)
        {
            _planes = planes;
            _random = random;
            _config = config;
        }

        public bool TrySelect(Vector3 origin, IReadOnlyList<Pickup> taken, out Vector3 position)
        {
            var bounds = _planes.RoomBounds;
            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var candidate = new Vector3(
                    _random.Range(bounds.min.x, bounds.max.x),
                    _planes.FloorHeight,
                    _random.Range(bounds.min.z, bounds.max.z));

                var distance = HorizontalDistance(origin, candidate);
                if (distance < _config.SpawnMinDistance || distance > _config.SpawnMaxDistance)
                    continue;
                if (IsCrowded(candidate, taken) || !_planes.IsFloorPoint(candidate))
                    continue;

                position = candidate;
                return true;
            }

            position = default;
            return false;
        }

        private bool IsCrowded(Vector3 candidate, IReadOnlyList<Pickup> taken)
        {
            for (var i = 0; i < taken.Count; i++)
            {
                if (HorizontalDistance(taken[i].Position, candidate) < _config.MinSpacing)
                    return true;
            }

            return false;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
