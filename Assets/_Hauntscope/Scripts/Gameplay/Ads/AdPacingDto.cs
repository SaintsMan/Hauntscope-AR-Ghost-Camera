using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Ads
{
    [Serializable]
    public sealed class AdPacingDto
    {
        [SerializeField] private int _version;
        [SerializeField] private int _huntsSinceInterstitial;
        // UTC ticks of the last ad; 0 = none yet.
        [SerializeField] private long _lastAdTicks;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public AdPacingDto()
        {
        }

        public AdPacingDto(int version, int huntsSinceInterstitial, long lastAdTicks)
        {
            _version = version;
            _huntsSinceInterstitial = huntsSinceInterstitial;
            _lastAdTicks = lastAdTicks;
        }

        public int Version => _version;

        public int HuntsSinceInterstitial => _huntsSinceInterstitial;

        public long LastAdTicks => _lastAdTicks;
    }
}
