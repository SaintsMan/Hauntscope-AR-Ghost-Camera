using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Engagement
{
    // One frame of the daily ration's cassette: ectoplasm, gear, or both.
    [Serializable]
    public sealed class LoginReward
    {
        [SerializeField, Min(0)] private int _ectoplasm;
        [SerializeField] private GearReward[] _gear = Array.Empty<GearReward>();

        public LoginReward()
        {
        }

        public LoginReward(int ectoplasm, params GearReward[] gear)
        {
            _ectoplasm = ectoplasm;
            _gear = gear;
        }

        public int Ectoplasm => _ectoplasm;

        public RewardBundle ToBundle()
        {
            return new RewardBundle(_ectoplasm, _gear);
        }
    }
}
