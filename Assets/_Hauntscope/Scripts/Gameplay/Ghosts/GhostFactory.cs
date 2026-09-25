using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostFactory
    {
        private readonly IPlaneProvider _planes;
        private readonly ICameraPose _camera;
        private readonly IRandom _random;
        private readonly GhostConfig _config;
        private readonly SpawnPointSelector _spawnPoints;

        public GhostFactory(
            IPlaneProvider planes,
            ICameraPose camera,
            IRandom random,
            GhostConfig config,
            SpawnPointSelector spawnPoints)
        {
            _planes = planes;
            _camera = camera;
            _random = random;
            _config = config;
            _spawnPoints = spawnPoints;
        }

        public Ghost Create(GhostData data)
        {
            var position = _spawnPoints.Select(_camera.Position);
            position.y = _planes.FloorHeight + _random.Range(data.Motion.HoverHeightMin, data.Motion.HoverHeightMax);

            var mover = new GhostMover(_planes, _config);
            mover.Teleport(position);

            var view = Object.Instantiate(data.Prefab, position, Quaternion.identity);
            view.name = data.Id;

            var context = new GhostContext(data.Motion, _config, mover, _random, _planes);
            var ghost = new Ghost(context, view);
            ghost.Start();
            return ghost;
        }
    }
}
