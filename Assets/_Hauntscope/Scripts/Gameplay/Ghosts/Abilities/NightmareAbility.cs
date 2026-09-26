using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The mara (GDD 5.32) is drawn to the light: while the lens is on her she comes at the player, in the dark she
    // backs off. Reaching the player in the light, she lunges, bites a chunk out of the battery and springs away, spent
    // for a moment. Under the beam she flees like any ghost.
    public sealed class NightmareAbility : IGhostAbility
    {
        private readonly float _litThreshold;
        private readonly float _lungeDistance;
        private readonly float _bite;
        private readonly float _bounceDistance;
        private readonly float _bounceDuration;
        private readonly float _spentStagger;
        private readonly float _retreatDistance;

        private float _elapsed;
        private Vector3 _from;
        private Vector3 _to;

        public NightmareAbility(float litThreshold, float lungeDistance, float bite, float bounceDistance, float bounceDuration,
            float spentStagger, float retreatDistance)
        {
            _litThreshold = litThreshold;
            _lungeDistance = lungeDistance;
            _bite = bite;
            _bounceDistance = bounceDistance;
            _bounceDuration = bounceDuration;
            _spentStagger = spentStagger;
            _retreatDistance = retreatDistance;
        }

        public bool IsBouncing { get; private set; }

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (IsBouncing)
            {
                Advance(ghost, deltaTime);
                return;
            }

            var camera = ghost.Context.Camera.Position;
            var lit = ghost.Reveal >= _litThreshold;
            var toPlayer = camera - ghost.Position;
            toPlayer.y = 0f;
            var away = toPlayer.sqrMagnitude > Mathf.Epsilon ? -toPlayer.normalized : Vector3.back;
            // Spent, she is the player's chance, not a threat.
            if (lit && !ghost.IsBeamed && !ghost.IsStaggered && toPlayer.magnitude <= _lungeDistance)
            {
                Begin(ghost, camera, away);
                return;
            }

            // Fleeing and hiding steer themselves; only a free ghost is pulled towards the light or pushed from it.
            if (!ghost.IsWandering && !ghost.IsAlerted)
                return;

            var target = lit ? camera : camera + away * _retreatDistance;
            target.y = ghost.Position.y;
            ghost.Context.Mover.SetTarget(target);
        }

        private void Begin(Ghost ghost, Vector3 camera, Vector3 away)
        {
            _from = ghost.Position;
            _to = camera + away * _bounceDistance;
            _to.y = _from.y;
            _elapsed = 0f;
            IsBouncing = true;
            ghost.Lunge();
            ghost.Bite(_bite);
        }

        private void Advance(Ghost ghost, float deltaTime)
        {
            _elapsed += deltaTime;
            var t = _bounceDuration > 0f ? Mathf.Clamp01(_elapsed / _bounceDuration) : 1f;
            var eased = 1f - (1f - t) * (1f - t);
            ghost.Context.Mover.Teleport(Vector3.Lerp(_from, _to, eased));
            if (t < 1f)
                return;

            IsBouncing = false;
            ghost.Stagger(_spentStagger);
        }
    }
}
