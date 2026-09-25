using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostMover
    {
        private const float FullCircle = Mathf.PI * 2f;

        private readonly IPlaneProvider _planes;
        private readonly GhostConfig _config;
        private Vector3 _velocity;
        private float _elapsed;

        public GhostMover(IPlaneProvider planes, GhostConfig config)
        {
            _planes = planes;
            _config = config;
        }

        public Vector3 Position { get; private set; }

        public Vector3 Target { get; private set; }

        public Vector3 VisualPosition =>
            Position + Vector3.up * (Mathf.Sin(_elapsed * _config.BobFrequency * FullCircle) * _config.BobAmplitude);

        public void Teleport(Vector3 position)
        {
            Position = ClampToRoom(position);
            Target = Position;
            _velocity = Vector3.zero;
        }

        public void SetTarget(Vector3 target)
        {
            Target = ClampToRoom(target);
        }

        public void Tick(float deltaTime, float maxSpeed)
        {
            _elapsed += deltaTime;
            var next = Vector3.SmoothDamp(Position, Target, ref _velocity, _config.MoveSmoothTime, maxSpeed, deltaTime);
            Position = ClampToRoom(next);
        }

        private Vector3 ClampToRoom(Vector3 position)
        {
            var bounds = _planes.RoomBounds;
            position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
            position.z = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);
            return position;
        }
    }
}
