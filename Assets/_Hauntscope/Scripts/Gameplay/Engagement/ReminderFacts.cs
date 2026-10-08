using System;

namespace Hauntscope.Gameplay.Engagement
{
    // What the planner needs to know about the player at the moment the game goes to the background.
    public readonly struct ReminderFacts
    {
        public ReminderFacts(DateTime localNow, bool rationClaimedToday, DateTime? offerEndsLocal, int unheardTapes = 0,
            int totalCaptures = 0)
        {
            UnheardTapes = unheardTapes;
            TotalCaptures = totalCaptures;
            LocalNow = localNow;
            RationClaimedToday = rationClaimedToday;
            OfferEndsLocal = offerEndsLocal;
        }

        public DateTime LocalNow { get; }

        public bool RationClaimedToday { get; }

        // The end of a running paid offer the player has not taken; null when there is none.
        public DateTime? OfferEndsLocal { get; }

        // Agent Vale's tapes unlocked but not listened to yet.
        public int UnheardTapes { get; }

        public int TotalCaptures { get; }
    }
}
