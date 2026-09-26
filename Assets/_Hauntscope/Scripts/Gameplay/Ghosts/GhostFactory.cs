using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts.Abilities;
using Hauntscope.Gameplay.Store;
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
        private readonly ScareConfig _scare;
        private readonly HuntModifiers _modifiers;

        public GhostFactory(
            IPlaneProvider planes,
            ICameraPose camera,
            IRandom random,
            GhostConfig config,
            SpawnPointSelector spawnPoints,
            ScareConfig scare,
            HuntModifiers modifiers)
        {
            _modifiers = modifiers;
            _planes = planes;
            _camera = camera;
            _random = random;
            _config = config;
            _spawnPoints = spawnPoints;
            _scare = scare;
        }

        public Ghost Create(GhostData data)
        {
            var position = _spawnPoints.Select(_camera.Position);
            position.y = _planes.FloorHeight + _random.Range(data.Motion.HoverHeightMin, data.Motion.HoverHeightMax);

            var mover = new GhostMover(_planes, _config);
            mover.Teleport(position);

            var view = Object.Instantiate(data.Prefab, position, Quaternion.identity);
            view.name = data.Id;
            view.SetRimColor(data.RimColor);

            var context = new GhostContext(data.Motion, data.Detection, data.Capture, _config, _scare, mover, _random, _planes, _camera);
            var ghost = new Ghost(context, view, CreateAbilities(data));
            ghost.SetSpeedModifiers(_modifiers.GhostSpeed, _modifiers.BeamedGhostSpeed);
            ghost.Start();
            return ghost;
        }

        private static IGhostAbility[] CreateAbilities(GhostData data)
        {
            var abilities = new IGhostAbility[data.Abilities.Count];
            for (var i = 0; i < abilities.Length; i++)
                abilities[i] = data.Abilities[i].CreateAbility();

            return abilities;
        }
    }
}
