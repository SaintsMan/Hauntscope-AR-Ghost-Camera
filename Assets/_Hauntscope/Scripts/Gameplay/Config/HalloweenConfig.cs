using System;
using Hauntscope.Gameplay.Season;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // The Halloween event of 1.0.4 (GDD 5.42): when it ends, how often its ghost comes, what one candy is worth and the
    // skins it buys. It only runs while Remote Config says season_event = "halloween"; every number here can be moved
    // from the console too (RemoteKeys), so the event can be tuned and cut short without a new build.
    [Serializable]
    public sealed class HalloweenConfig
    {
        [SerializeField, Range(2020, 2100)] private int _endYear = 2026;
        [SerializeField, Range(1, 12)] private int _endMonth = 11;
        [SerializeField, Range(1, 31)] private int _endDay = 2;
        [SerializeField, Range(0, 100)] private int _jackPercent = 35;
        [SerializeField, Min(1)] private int _candyPerPickup = 1;
        [SerializeField, Min(0)] private int _welcomeCandy = 10;
        [SerializeField, Min(1)] private int _jackFromHunt = 2;
        [SerializeField] private HalloweenSkin[] _skins = Array.Empty<HalloweenSkin>();

        public HalloweenConfig()
        {
        }

        public HalloweenConfig(int jackPercent, int candyPerPickup, HalloweenSkin[] skins, int welcomeCandy = 0, int jackFromHunt = 3)
        {
            _welcomeCandy = welcomeCandy;
            _jackFromHunt = jackFromHunt;
            _jackPercent = jackPercent;
            _candyPerPickup = candyPerPickup;
            _skins = skins;
        }

        // The last day of the event, inclusive: candy burns the day after.
        public DateTime EndDate => new DateTime(_endYear, _endMonth, _endDay);

        // Chance, in percent, that a hunt from JackFromHunt on is against the event ghost.
        public int JackPercent => _jackPercent;

        // The first hunt (1-based) the event ghost may come to; the first one always teaches with the wisp.
        public int JackFromHunt => _jackFromHunt;

        // Candy every player finds in the purse once while the event runs, so the cheapest skin is a hunt or two away.
        public int WelcomeCandy => _welcomeCandy;

        public int CandyPerPickup => _candyPerPickup;

        public HalloweenSkin[] Skins => _skins;
    }
}
