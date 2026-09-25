using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class SpawnPointSelector
    {
        private const int MaxAttempts = 30;

        private readonly IPlaneProvider _planes;
        private readonly IRandom _random;
        private readonly RoomConfig _config;

        public SpawnPointSelector(IPlaneProvider planes, IRandom random, RoomConfig config)
        {
            _planes = planes;
            _random = random;
            _config = config;
        }

        public Vector3 Select(Vector3 origin)
        {
            var bounds = _planes.RoomBounds;
            var floor = _planes.FloorHeight;

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var candidate = new Vector3(
                    _random.Range(bounds.min.x, bounds.max.x),
                    floor,
                    _random.Range(bounds.min.z, bounds.max.z));

                var distance = HorizontalDistance(origin, candidate);
                if (distance >= _config.SpawnMinDistance && distance <= _config.SpawnMaxDistance)
                    return candidate;
            }

            return FarthestCorner(origin, bounds, floor);
        }

        private static Vector3 FarthestCorner(Vector3 origin, Bounds bounds, float floor)
        {
            var x = Mathf.Abs(bounds.min.x - origin.x) > Mathf.Abs(bounds.max.x - origin.x) ? bounds.min.x : bounds.max.x;
            var z = Mathf.Abs(bounds.min.z - origin.z) > Mathf.Abs(bounds.max.z - origin.z) ? bounds.min.z : bounds.max.z;
            return new Vector3(x, floor, z);
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
