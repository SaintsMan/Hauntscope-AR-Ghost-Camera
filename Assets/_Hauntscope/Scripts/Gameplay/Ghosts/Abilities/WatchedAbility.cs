using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The lurker (GDD 5.28): it only moves while the player looks elsewhere and creeps up behind them, closing in
    // slowly once it is there. Held in view it freezes; turned on while it has only just set off, it is caught out
    // and winded. Left at the player's back too long, it lunges and springs away across the room.
    public sealed class WatchedAbility : IGhostAbility
    {
        private readonly float _viewAngle;
        private readonly float _behindDistance;
        private readonly float _closeInSpeed;
        private readonly float _lungeDistance;
        private readonly float _lungeDelay;
        private readonly float _bounceDistance;
        private readonly float _bounceDuration;
        private readonly float _catchWindow;
        private readonly float _catchCooldown;
        private readonly float _staggerDuration;
        private readonly float _creakInterval;

        private bool _isMoving;
        private bool _hasBeenWatched;
        private float _movingTime;
        private float _closeTime;
        private float _closeIn;
        private float _catchCooldownRemaining;
        private float _creakCooldownRemaining;
        private bool _isBouncing;
        private float _bounceElapsed;
        private Vector3 _bounceFrom;
        private Vector3 _bounceTo;

        public WatchedAbility(float viewAngle, float behindDistance, float closeInSpeed, float lungeDistance, float lungeDelay,
            float bounceDistance, float bounceDuration, float catchWindow, float catchCooldown, float staggerDuration, float creakInterval)
        {
            _viewAngle = viewAngle;
            _behindDistance = behindDistance;
            _closeInSpeed = closeInSpeed;
            _lungeDistance = lungeDistance;
            _lungeDelay = lungeDelay;
            _bounceDistance = bounceDistance;
            _bounceDuration = bounceDuration;
            _catchWindow = catchWindow;
            _catchCooldown = catchCooldown;
            _staggerDuration = staggerDuration;
            _creakInterval = creakInterval;
        }

        public bool IsBouncing => _isBouncing;

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (_isBouncing)
            {
                AdvanceBounce(ghost, deltaTime);
                return;
            }

            _catchCooldownRemaining = Mathf.Max(0f, _catchCooldownRemaining - deltaTime);
            _creakCooldownRemaining = Mathf.Max(0f, _creakCooldownRemaining - deltaTime);
            var camera = ghost.Context.Camera;
            var watched = IsWatched(ghost.Position, camera.Position, camera.Forward);
            ghost.Hold(watched);

            if (watched)
            {
                if (_isMoving && _movingTime <= _catchWindow && _catchCooldownRemaining <= 0f)
                {
                    ghost.Stagger(_staggerDuration);
                    _catchCooldownRemaining = _catchCooldown;
                }

                _hasBeenWatched = true;
                _isMoving = false;
                _closeTime = 0f;
                return;
            }

            if (!_isMoving)
                SetOff(ghost);

            _movingTime += deltaTime;
            if (ghost.IsWandering || ghost.IsAlerted)
                Stalk(ghost, camera.Position, camera.Forward, deltaTime);

            if (HorizontalDistance(ghost.Position, camera.Position) > _lungeDistance)
            {
                _closeTime = 0f;
                return;
            }

            _closeTime += deltaTime;
            if (_closeTime >= _lungeDelay)
                BeginBounce(ghost, camera.Position);
        }

        private bool IsWatched(Vector3 position, Vector3 cameraPosition, Vector3 cameraForward)
        {
            var toGhost = position - cameraPosition;
            return toGhost.sqrMagnitude <= 0f || Vector3.Angle(cameraForward, toGhost) <= _viewAngle;
        }

        // The creak only means something once the player has already turned away from it at least once.
        private void SetOff(Ghost ghost)
        {
            _isMoving = true;
            _movingTime = 0f;
            if (!_hasBeenWatched || _creakCooldownRemaining > 0f)
                return;

            ghost.Creep();
            _creakCooldownRemaining = _creakInterval;
        }

        private void Stalk(Ghost ghost, Vector3 cameraPosition, Vector3 cameraForward, float deltaTime)
        {
            var back = new Vector3(-cameraForward.x, 0f, -cameraForward.z);
            if (back.sqrMagnitude <= 0f)
                back = Vector3.back;
            back.Normalize();

            // It first takes up its place behind the player, then inches closer for as long as nobody turns round.
            var distance = HorizontalDistance(ghost.Position, cameraPosition);
            if (distance <= _behindDistance + _lungeDistance * 0.5f)
                _closeIn = Mathf.Min(_behindDistance, _closeIn + _closeInSpeed * deltaTime);

            var target = cameraPosition + back * Mathf.Max(0f, _behindDistance - _closeIn);
            target.y = ghost.Position.y;
            ghost.Context.Mover.SetTarget(target);
        }

        private void BeginBounce(Ghost ghost, Vector3 cameraPosition)
        {
            var away = ghost.Position - cameraPosition;
            away.y = 0f;
            if (away.sqrMagnitude <= 0f)
                away = Vector3.back;

            _bounceFrom = ghost.Position;
            _bounceTo = cameraPosition + away.normalized * _bounceDistance;
            _bounceTo.y = _bounceFrom.y;
            _bounceElapsed = 0f;
            _isBouncing = true;
            _closeTime = 0f;
            _closeIn = 0f;
            ghost.Lunge();
        }

        private void AdvanceBounce(Ghost ghost, float deltaTime)
        {
            _bounceElapsed += deltaTime;
            var t = _bounceDuration > 0f ? Mathf.Clamp01(_bounceElapsed / _bounceDuration) : 1f;
            var eased = 1f - (1f - t) * (1f - t);
            ghost.Context.Mover.Teleport(Vector3.Lerp(_bounceFrom, _bounceTo, eased));
            if (t >= 1f)
                _isBouncing = false;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
