using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "SkittishAbility", menuName = "Hauntscope/Abilities/Skittish")]
    public sealed class SkittishAbilityConfig : GhostAbilityConfig
    {
        [SerializeField, Min(0f)] private float _sharpSpeed = 0.5f;
        [SerializeField, Min(0f)] private float _sharpTurn = 90f;
        [SerializeField, Min(0f)] private float _calmSpeed = 0.1f;
        [SerializeField, Min(0f)] private float _calmTurn = 15f;
        [SerializeField, Min(0f)] private float _calmTime = 1.5f;
        [SerializeField, Min(0f)] private float _sitDistance = 1.2f;
        [SerializeField, Min(0f)] private float _sitDuration = 3f;
        [SerializeField, Min(0f)] private float _arriveDistance = 0.25f;
        [SerializeField, Min(0f)] private float _smoothing = 0.15f;
        [SerializeField, Min(0f)] private float _sitCooldown = 4f;

        public override IGhostAbility CreateAbility()
        {
            return new SkittishAbility(_sharpSpeed, _sharpTurn, _calmSpeed, _calmTurn, _calmTime, _sitDistance, _sitDuration,
                _arriveDistance, _smoothing, _sitCooldown);
        }
    }
}
