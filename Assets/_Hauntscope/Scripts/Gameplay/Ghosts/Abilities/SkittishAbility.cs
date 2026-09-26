using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The phantom cat (GDD 5.28): a sudden move or a quick turn sends it running, but stand still for a moment and it
    // comes over and sits down by the player. Sitting is its opening; the next sudden move ends it.
    public sealed class SkittishAbility : IGhostAbility
    {
        private readonly float _sharpSpeed;
        private readonly float _sharpTurn;
        private readonly float _calmSpeed;
        private readonly float _calmTurn;
        private readonly float _calmTime;
        private readonly float _sitDistance;
        private readonly float _sitDuration;
        private readonly float _arriveDistance;
        private readonly float _smoothing;
        private readonly float _sitCooldown;

        private bool _hasPose;
        private Vector3 _lastPosition;
        private Vector3 _lastForward;
        private float _speed;
        private float _turn;
        private float _stillFor;
        private float _sitCooldownRemaining;

        public SkittishAbility(float sharpSpeed, float sharpTurn, float calmSpeed, float calmTurn, float calmTime, float sitDistance,
            float sitDuration, float arriveDistance, float smoothing, float sitCooldown)
        {
            _sharpSpeed = sharpSpeed;
            _sharpTurn = sharpTurn;
            _calmSpeed = calmSpeed;
            _calmTurn = calmTurn;
            _calmTime = calmTime;
            _sitDistance = sitDistance;
            _sitDuration = sitDuration;
            _arriveDistance = arriveDistance;
            _smoothing = smoothing;
            _sitCooldown = sitCooldown;
        }

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            var camera = ghost.Context.Camera;
            Measure(camera.Position, camera.Forward, deltaTime);
            _sitCooldownRemaining = Mathf.Max(0f, _sitCooldownRemaining - deltaTime);

            if (_speed > _sharpSpeed || _turn > _sharpTurn)
            {
                _stillFor = 0f;
                ghost.Startle();
                return;
            }

            _stillFor = _speed < _calmSpeed && _turn < _calmTurn ? _stillFor + deltaTime : 0f;
            if (_stillFor < _calmTime || ghost.IsSettled || !(ghost.IsWandering || ghost.IsAlerted))
                return;

            var spot = SitSpot(ghost.Position, camera.Position);
            ghost.Context.Mover.SetTarget(spot);
            if (_sitCooldownRemaining > 0f || HorizontalDistance(ghost.Position, spot) > _arriveDistance)
                return;

            ghost.Settle(_sitDuration);
            _sitCooldownRemaining = _sitDuration + _sitCooldown;
        }

        // Camera speed and turn rate, smoothed so the jitter of a hand-held phone does not read as a sudden move.
        private void Measure(Vector3 position, Vector3 forward, float deltaTime)
        {
            if (!_hasPose)
            {
                _hasPose = true;
                _lastPosition = position;
                _lastForward = forward;
                return;
            }

            var speed = Vector3.Distance(position, _lastPosition) / deltaTime;
            var turn = Vector3.Angle(_lastForward, forward) / deltaTime;
            var blend = _smoothing > 0f ? 1f - Mathf.Exp(-deltaTime / _smoothing) : 1f;
            _speed = Mathf.Lerp(_speed, speed, blend);
            _turn = Mathf.Lerp(_turn, turn, blend);
            _lastPosition = position;
            _lastForward = forward;
        }

        private Vector3 SitSpot(Vector3 cat, Vector3 player)
        {
            var fromPlayer = cat - player;
            fromPlayer.y = 0f;
            if (fromPlayer.sqrMagnitude <= 0f)
                fromPlayer = Vector3.forward;

            var spot = player + fromPlayer.normalized * _sitDistance;
            spot.y = cat.y;
            return spot;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
