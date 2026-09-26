using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The kaidannyk (GDD 5.32) fights the beam with its chains: at set points of the capture it yanks sideways, and if
    // the ring has lost it when the yank ends, the capture slips back. Dropped below a point it has used, it re-arms.
    public sealed class ShackledAbility : IGhostAbility
    {
        private const float MinSideRoom = 0.4f;

        private readonly float[] _thresholds;
        private readonly float _distance;
        private readonly float _duration;
        private readonly float _gripLoss;
        private readonly float _rearm;

        private int _next;
        private float _elapsed;
        private Vector3 _from;
        private Vector3 _to;

        public ShackledAbility(float[] thresholds, float distance, float duration, float gripLoss, float rearm)
        {
            _thresholds = thresholds;
            _distance = distance;
            _duration = duration;
            _gripLoss = gripLoss;
            _rearm = rearm;
        }

        public bool IsYanking { get; private set; }

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (IsYanking)
            {
                Advance(ghost, deltaTime);
                return;
            }

            if (_next > 0 && ghost.CaptureProgress < _thresholds[_next - 1] - _rearm)
                _next--;

            if (_next < _thresholds.Length && ghost.IsBeamed && ghost.CaptureProgress >= _thresholds[_next])
                Begin(ghost);
        }

        private void Begin(Ghost ghost)
        {
            _next++;
            var context = ghost.Context;
            var side = RoomReach.Sideways(context, MinSideRoom);
            _from = ghost.Position;
            _to = _from + side * Mathf.Min(_distance, RoomReach.Distance(context, side));
            _elapsed = 0f;
            IsYanking = true;
            ghost.DashTo(_to);
        }

        private void Advance(Ghost ghost, float deltaTime)
        {
            _elapsed += deltaTime;
            var t = _duration > 0f ? Mathf.Clamp01(_elapsed / _duration) : 1f;
            var eased = 1f - (1f - t) * (1f - t);
            ghost.Context.Mover.Teleport(Vector3.Lerp(_from, _to, eased));
            if (t < 1f)
                return;

            IsYanking = false;
            ghost.TestGrip(_gripLoss);
        }
    }
}
