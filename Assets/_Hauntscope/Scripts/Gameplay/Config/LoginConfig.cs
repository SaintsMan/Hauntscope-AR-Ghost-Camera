using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Engagement;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.30: the daily ration, a cassette of seven frames.
    [Serializable]
    public sealed class LoginConfig
    {
        [SerializeField] private LoginReward[] _days = Array.Empty<LoginReward>();
        [SerializeField, Min(1)] private int _adMultiplier = 2;

        public LoginConfig()
        {
        }

        public LoginConfig(LoginReward[] days, int adMultiplier)
        {
            _days = days;
            _adMultiplier = adMultiplier;
        }

        public IReadOnlyList<LoginReward> Days => _days;

        // A rewarded ad multiplies the ectoplasm of a day by this; gear is never multiplied.
        public int AdMultiplier => _adMultiplier;
    }
}
