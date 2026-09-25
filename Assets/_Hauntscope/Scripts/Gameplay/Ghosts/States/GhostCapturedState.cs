using Hauntscope.Core.StateMachines;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.States
{
    public sealed class GhostCapturedState : IState
    {
        private const float FullCircle = Mathf.PI * 2f;

        private readonly GhostContext _context;
        private Vector3 _start;

        public GhostCapturedState(GhostContext context)
        {
            _context = context;
        }

        public float Progress { get; private set; }

        public bool IsFinished => Progress >= 1f;

        public void Enter()
        {
            Progress = 0f;
            _start = _context.Mover.Position;
        }

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
            if (IsFinished)
                return;

            var config = _context.Config;
            Progress = Mathf.Clamp01(Progress + deltaTime / config.CaptureDuration);

            var camera = _context.Camera;
            var eased = Progress * Progress;
            var center = Vector3.Lerp(_start, camera.Position, eased);
            var angle = Progress * config.CaptureSpiralTurns * FullCircle;
            var radius = config.CaptureSpiralRadius * (1f - Progress);
            var side = Vector3.Cross(Vector3.up, camera.Forward).normalized;
            var offset = (side * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * radius;

            _context.Mover.MoveTo(center + offset);
            _context.Mover.FaceTowards(camera.Position);
        }
    }
}
