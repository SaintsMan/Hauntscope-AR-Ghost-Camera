using System;

namespace Hauntscope.Gameplay.Ads
{
    // When ads last ran. Kept between launches (AdPacingRepository): most agents play one or two hunts a session, and
    // a count that restarted with the game let them meet an interstitial almost never (36 shown of 646 filled).
    public sealed class AdPacing
    {
        public AdPacing()
        {
        }

        public AdPacing(int huntsSinceInterstitial, DateTime? lastAdAt)
        {
            HuntsSinceInterstitial = Math.Max(0, huntsSinceInterstitial);
            LastAdAt = lastAdAt;
        }

        public event Action Changed;

        public int HuntsSinceInterstitial { get; private set; }

        public DateTime? LastAdAt { get; private set; }

        public void RegisterHunt()
        {
            HuntsSinceInterstitial++;
            Changed?.Invoke();
        }

        public void MarkShown(DateTime utcNow, bool interstitial)
        {
            LastAdAt = utcNow;
            if (interstitial)
                HuntsSinceInterstitial = 0;
            Changed?.Invoke();
        }
    }
}
