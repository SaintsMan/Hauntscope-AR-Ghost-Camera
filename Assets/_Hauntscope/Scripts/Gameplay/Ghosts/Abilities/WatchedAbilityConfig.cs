using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "WatchedAbility", menuName = "Hauntscope/Abilities/Watched")]
    public sealed class WatchedAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Range(1f, 90f)] private float _viewAngle = 50f;
        [SerializeField, Min(0f)] private float _behindDistance = 1.5f;
        [SerializeField, Min(0f)] private float _closeInSpeed = 0.15f;
        [SerializeField, Min(0f)] private float _lungeDistance = 0.9f;
        [SerializeField, Min(0f)] private float _lungeDelay = 1f;
        [SerializeField, Min(0f)] private float _bounceDistance = 3f;
        [SerializeField, Min(0f)] private float _bounceDuration = 0.35f;
        [SerializeField, Min(0f)] private float _catchWindow = 0.5f;
        [SerializeField, Min(0f)] private float _catchCooldown = 3f;
        [SerializeField, Min(0f)] private float _staggerDuration = 1.2f;
        [SerializeField, Min(0f)] private float _creakInterval = 2f;

        public override IGhostAbility CreateAbility()
        {
            return new WatchedAbility(_viewAngle, _behindDistance, _closeInSpeed, _lungeDistance, _lungeDelay, _bounceDistance,
                _bounceDuration, _catchWindow, _catchCooldown, _staggerDuration, _creakInterval);
        }
    }
}
