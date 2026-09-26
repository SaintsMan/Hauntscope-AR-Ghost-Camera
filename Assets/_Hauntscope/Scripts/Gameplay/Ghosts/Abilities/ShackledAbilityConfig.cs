using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "ShackledAbility", menuName = "Hauntscope/Abilities/Shackled")]
    public sealed class ShackledAbilityConfig : GhostAbilityConfig
    {
        // Both below CaptureConfig.SurgeThreshold: abilities fall silent during the surge, so a yank there never comes.
        [SerializeField] private float[] _thresholds = { 0.4f, 0.7f };
        [SerializeField, Min(0f)] private float _distance = 0.6f;
        [SerializeField, Min(0.01f)] private float _duration = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _gripLoss = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _rearm = 0.15f;

        public override IGhostAbility CreateAbility()
        {
            return new ShackledAbility(_thresholds, _distance, _duration, _gripLoss, _rearm);
        }
    }
}
