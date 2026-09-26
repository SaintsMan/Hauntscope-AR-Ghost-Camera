using System.Collections.Generic;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Hunt
{
    // What happened in one finished hunt, as the agency's contracts read it (GDD 5.29).
    public sealed class HuntReport
    {
        public HuntReport(
            bool isCaptured,
            GhostData ghost,
            float duration,
            int staggerHits,
            IReadOnlyList<int> photoStars,
            IReadOnlyList<string> pickups,
            int flushOuts,
            float captureDistance,
            float batteryLeft,
            bool usedSpareBattery,
            bool usedBoosters,
            int shiftRound)
        {
            IsCaptured = isCaptured;
            Ghost = ghost;
            Duration = duration;
            StaggerHits = staggerHits;
            PhotoStars = photoStars;
            Pickups = pickups;
            FlushOuts = flushOuts;
            CaptureDistance = captureDistance;
            BatteryLeft = batteryLeft;
            UsedSpareBattery = usedSpareBattery;
            UsedBoosters = usedBoosters;
            ShiftRound = shiftRound;
        }

        public bool IsCaptured { get; }

        public GhostData Ghost { get; }

        public float Duration { get; }

        // Times the beam held a vulnerable ghost long enough to count as a hit.
        public int StaggerHits { get; }

        // Stars of every photo taken, in order.
        public IReadOnlyList<int> PhotoStars { get; }

        // Ids of everything picked up around the room.
        public IReadOnlyList<string> Pickups { get; }

        // Times the ghost was caught in its hiding spot and driven out.
        public int FlushOuts { get; }

        // Camera-to-ghost distance when the capture completed.
        public float CaptureDistance { get; }

        // Battery left at the end, 0..1.
        public float BatteryLeft { get; }

        public bool UsedSpareBattery { get; }

        public bool UsedBoosters { get; }

        // 1-based round of the night shift this hunt was, 0 outside a shift.
        public int ShiftRound { get; }

        public int CountPhotos(int minStars)
        {
            var count = 0;
            foreach (var stars in PhotoStars)
            {
                if (stars >= minStars)
                    count++;
            }

            return count;
        }

        // An empty id counts every pickup.
        public int CountPickups(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Pickups.Count;

            var count = 0;
            foreach (var pickup in Pickups)
            {
                if (pickup == id)
                    count++;
            }

            return count;
        }
    }
}
