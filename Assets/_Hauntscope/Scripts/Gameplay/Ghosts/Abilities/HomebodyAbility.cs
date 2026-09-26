using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The domovyk (GDD 5.32) lives in the furniture: it hides whenever it can (its own HideConfig does that) and,
    // hidden, knocks now and then, so the player finds it by ear before the cold spot. It never lunges at the camera:
    // where another ghost would jump-scare, it rattles the crockery instead.
    public sealed class HomebodyAbility : IGhostAbility
    {
        private readonly float _intervalMin;
        private readonly float _intervalMax;
        private readonly float _firstKnock;

        private float _untilKnock;
        private bool _wasHiding;
        private float _elapsed;
        private bool _hasRattled;

        public HomebodyAbility(float intervalMin, float intervalMax, float firstKnock)
        {
            _intervalMin = intervalMin;
            _intervalMax = intervalMax;
            _firstKnock = firstKnock;
        }

        public void Tick(Ghost ghost, float deltaTime)
        {
            _elapsed += deltaTime;
            TryRattle(ghost);
            if (!ghost.IsHiding)
            {
                _wasHiding = false;
                return;
            }

            // The first knock comes soon after it settles in, so a player who saw it vanish can follow at once.
            if (!_wasHiding)
            {
                _wasHiding = true;
                _untilKnock = _firstKnock;
            }

            _untilKnock -= deltaTime;
            if (_untilKnock > 0f)
                return;

            ghost.Knock();
            _untilKnock = ghost.Context.Random.Range(_intervalMin, _intervalMax);
        }

        // The same moment ScarePolicy picks for a jump scare, once per hunt, but only the noise.
        private void TryRattle(Ghost ghost)
        {
            var scare = ghost.Context.Scare;
            if (_hasRattled || !ghost.IsAlerted || _elapsed < scare.MinTime)
                return;

            var camera = ghost.Context.Camera;
            var toGhost = ghost.Position - camera.Position;
            if (toGhost.magnitude > scare.Distance || Vector3.Angle(camera.Forward, toGhost) > scare.Angle)
                return;

            _hasRattled = true;
            ghost.Rattle();
        }
    }
}
