using System;

namespace Hauntscope.Gameplay.Engagement
{
    // What the planner needs to know about the player at the moment the game goes to the background.
    public readonly struct ReminderFacts
    {
        public ReminderFacts(DateTime localNow, bool rationClaimedToday, DateTime? offerEndsLocal)
        {
            LocalNow = localNow;
            RationClaimedToday = rationClaimedToday;
            OfferEndsLocal = offerEndsLocal;
        }

        public DateTime LocalNow { get; }

        public bool RationClaimedToday { get; }

        // The end of a running paid offer the player has not taken; null when there is none.
        public DateTime? OfferEndsLocal { get; }
    }
}
