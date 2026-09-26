using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.28: the witching hour, by the player's local clock.
    [Serializable]
    public sealed class NightConfig
    {
        [SerializeField, Range(0, 23)] private int _startHour = 23;
        [SerializeField, Range(0, 23)] private int _endHour = 4;
        [SerializeField, Min(0f)] private float _legendaryWeightMultiplier = 2f;
        [SerializeField, Min(1f)] private float _rewardMultiplier = 1.25f;

        public NightConfig()
        {
        }

        public NightConfig(int startHour, int endHour, float legendaryWeightMultiplier, float rewardMultiplier)
        {
            _startHour = startHour;
            _endHour = endHour;
            _legendaryWeightMultiplier = legendaryWeightMultiplier;
            _rewardMultiplier = rewardMultiplier;
        }

        public int StartHour => _startHour;

        // Exclusive: at 04:00 the night is over.
        public int EndHour => _endHour;

        public float LegendaryWeightMultiplier => _legendaryWeightMultiplier;

        public float RewardMultiplier => _rewardMultiplier;
    }
}
