using System;

namespace Hauntscope.Gameplay.Engagement
{
    public readonly struct PlannedReminder
    {
        public PlannedReminder(string kind, DateTime fireTime)
        {
            Kind = kind;
            FireTime = fireTime;
        }

        // Also the localization key stem: notify.<kind>.<variant>.title / .body
        public string Kind { get; }

        // Local wall-clock time.
        public DateTime FireTime { get; }
    }
}
