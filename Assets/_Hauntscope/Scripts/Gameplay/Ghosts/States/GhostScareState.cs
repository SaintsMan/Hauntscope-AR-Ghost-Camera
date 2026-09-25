using Hauntscope.Core.StateMachines;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.States
{
    // Rush into the player's face, tremble there, then pull back to where the lunge started.
    public sealed class GhostScareState : IState
    {
        private const float RetreatShare = 0.25f;
        private const float ShakeAmplitude = 0.02f;
        private const float ShakeFrequencyX = 47f;
        private const float ShakeFrequencyY = 53f;

        private readonly GhostContext _context;
        private Vector3 _start;
        private float _elapsed;

        public GhostScareState(GhostContext context)
        {
            _context = context;
        }

        public bool IsFinished => _elapsed >= _context.Scare.Duration;

        public void Enter()
        {
            _elapsed = 0f;
            _start = _context.Mover.Position;
            _context.Mover.Stop();
        }

        public void Exit()
        {
            _context.Mover.MoveTo(_start);
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            var config = _context.Scare;
            var camera = _context.Camera;
            var face = camera.Position + camera.Forward * config.FaceDistance;
            var retreatStart = config.Duration * (1f - RetreatShare);

            Vector3 position;
            if (_elapsed < config.RushTime)
            {
                position = Vector3.Lerp(_start, face, Ease(_elapsed / config.RushTime));
            }
            else if (_elapsed < retreatStart)
            {
                var shake = new Vector3(Mathf.Sin(_elapsed * ShakeFrequencyX), Mathf.Sin(_elapsed * ShakeFrequencyY) * 0.5f, 0f);
                position = face + shake * ShakeAmplitude;
            }
            else
            {
                var t = (_elapsed - retreatStart) / (config.Duration - retreatStart);
                position = Vector3.Lerp(face, _start, Ease(Mathf.Clamp01(t)));
            }

            _context.Mover.MoveTo(position);
            _context.Mover.FaceTowards(camera.Position);
        }

        private static float Ease(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}
