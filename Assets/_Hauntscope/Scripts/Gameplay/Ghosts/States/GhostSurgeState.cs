using Hauntscope.Core.StateMachines;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.States
{
    // The last fight before a capture: the ghost jerks around its spot in the camera plane, so the player has to
    // keep it in the ring through the thrashing rather than just keep holding.
    public sealed class GhostSurgeState : IState
    {
        private readonly GhostContext _context;
        private Vector3 _anchor;
        private Vector3 _from;
        private Vector3 _to;
        private float _elapsed;
        private float _jerkElapsed;

        public GhostSurgeState(GhostContext context)
        {
            _context = context;
        }

        public bool IsFinished => _elapsed >= _context.CaptureConfig.SurgeDuration;

        public void Enter()
        {
            _elapsed = 0f;
            _anchor = _context.Mover.Position;
            _from = _anchor;
            _to = _anchor;
            _jerkElapsed = _context.CaptureConfig.SurgeJerkInterval;
            _context.Mover.Stop();
        }

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            _jerkElapsed += deltaTime;

            var interval = _context.CaptureConfig.SurgeJerkInterval;
            if (_jerkElapsed >= interval)
            {
                _jerkElapsed = 0f;
                _from = _context.Mover.Position;
                _to = PickJerk();
            }

            var t = Mathf.Clamp01(_jerkElapsed / interval);
            var eased = 1f - (1f - t) * (1f - t) * (1f - t);
            _context.Mover.Teleport(Vector3.Lerp(_from, _to, eased));
            _context.Mover.FaceTowards(_context.Camera.Position);
        }

        private Vector3 PickJerk()
        {
            var camera = _context.Camera;
            var side = Vector3.Cross(Vector3.up, camera.Forward).normalized;
            var random = _context.Random;
            var offset = side * random.Range(-1f, 1f) + Vector3.up * random.Range(-1f, 1f);
            return _anchor + offset * _context.Capture.SurgeJerk;
        }
    }
}
