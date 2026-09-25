using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The wraith refuses to be held in the reticle: once beamed it darts sideways across the line of sight,
    // so the player has to re-aim instead of just holding the button.
    public sealed class DashAbility : IGhostAbility
    {
        private const float MinSideRoom = 0.5f;

        private readonly float _distance;
        private readonly float _duration;
        private readonly float _cooldown;
        private float _cooldownRemaining;
        private float _elapsed;
        private bool _isDashing;
        private Vector3 _from;
        private Vector3 _to;

        public DashAbility(float distance, float duration, float cooldown)
        {
            _distance = distance;
            _duration = duration;
            _cooldown = cooldown;
        }

        public bool IsDashing => _isDashing;

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (_isDashing)
            {
                Advance(ghost, deltaTime);
                return;
            }

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
                return;
            }

            if (ghost.IsBeamed)
                Begin(ghost);
        }

        private void Begin(Ghost ghost)
        {
            var context = ghost.Context;
            var lineOfSight = ghost.Position - context.Camera.Position;
            lineOfSight.y = 0f;
            if (lineOfSight.sqrMagnitude <= Mathf.Epsilon)
                lineOfSight = new Vector3(context.Camera.Forward.x, 0f, context.Camera.Forward.z);

            var side = new Vector3(lineOfSight.z, 0f, -lineOfSight.x).normalized;
            if (context.Random.Value < 0.5f)
                side = -side;

            // A dash into a wall would barely move; the other side of the line of sight is taken instead.
            if (Room(context, side) < MinSideRoom)
                side = -side;

            _from = ghost.Position;
            _to = _from + side * Mathf.Min(_distance, Room(context, side));
            _elapsed = 0f;
            _isDashing = true;
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

            _isDashing = false;
            _cooldownRemaining = _cooldown;
        }

        private static float Room(GhostContext context, Vector3 direction)
        {
            var bounds = context.Planes.RoomBounds;
            var position = context.Mover.Position;
            var room = float.MaxValue;
            if (direction.x > Mathf.Epsilon)
                room = Mathf.Min(room, (bounds.max.x - position.x) / direction.x);
            else if (direction.x < -Mathf.Epsilon)
                room = Mathf.Min(room, (bounds.min.x - position.x) / direction.x);
            if (direction.z > Mathf.Epsilon)
                room = Mathf.Min(room, (bounds.max.z - position.z) / direction.z);
            else if (direction.z < -Mathf.Epsilon)
                room = Mathf.Min(room, (bounds.min.z - position.z) / direction.z);

            return Mathf.Max(0f, room);
        }
    }
}
