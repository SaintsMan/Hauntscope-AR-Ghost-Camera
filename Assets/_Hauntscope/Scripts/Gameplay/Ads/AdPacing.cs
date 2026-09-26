using System;

namespace Hauntscope.Gameplay.Ads
{
    // Memory of when ads ran in this app session. Kept in memory on purpose: a restarted game starts fresh,
    // which only ever means fewer interstitials.
    public sealed class AdPacing
    {
        public int HuntsSinceInterstitial { get; private set; }

        public DateTime? LastAdAt { get; private set; }

        public void RegisterHunt()
        {
            HuntsSinceInterstitial++;
        }

        public void MarkShown(DateTime utcNow, bool interstitial)
        {
            LastAdAt = utcNow;
            if (interstitial)
                HuntsSinceInterstitial = 0;
        }
    }
}
