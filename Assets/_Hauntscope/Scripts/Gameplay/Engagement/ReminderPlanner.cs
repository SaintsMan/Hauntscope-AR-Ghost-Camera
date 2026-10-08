using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Engagement
{
    // Decides which reminders to leave behind when the game goes to the background (GDD 5.36): the daily ration, a
    // nudge after a few days away and the last hours of a paid offer. None at night, none within a few hours of
    // another; pure, so the whole schedule is testable.
    public sealed class ReminderPlanner
    {
        public const string Ration = "ration";
        public const string Offer = "offer";
        public const string ReturnPrefix = "return_";

        private readonly NotificationConfig _config;
        private readonly List<PlannedReminder> _candidates = new List<PlannedReminder>();

        public ReminderPlanner(NotificationConfig config)
        {
            _config = config;
        }

        public void Plan(ReminderFacts facts, List<PlannedReminder> result)
        {
            result.Clear();
            _candidates.Clear();
            var now = facts.LocalNow;
            var earliest = now.AddMinutes(_config.MinLeadMinutes);

            var evening = now.Date.AddHours(_config.EveningHour);
            var ration = !facts.RationClaimedToday && evening >= earliest ? evening : now.Date.AddDays(1).AddHours(_config.MorningHour);
            _candidates.Add(new PlannedReminder(Ration, OutOfQuietHours(ration)));

            foreach (var days in _config.ReturnDays)
                _candidates.Add(new PlannedReminder(ReturnPrefix + days, OutOfQuietHours(now.AddDays(days))));

            if (facts.OfferEndsLocal.HasValue)
            {
                // Moving it out of the night could push it past the end of the offer, so a night-time warning is dropped.
                var warning = facts.OfferEndsLocal.Value.AddHours(-_config.OfferWarningHours);
                if (warning >= earliest && !IsQuiet(warning))
                    _candidates.Add(new PlannedReminder(Offer, warning));
            }

            _candidates.Sort((a, b) => a.FireTime.CompareTo(b.FireTime));
            var gap = TimeSpan.FromHours(_config.MinGapHours);
            foreach (var candidate in _candidates)
            {
                if (candidate.FireTime < earliest)
                    continue;
                if (result.Count > 0 && candidate.FireTime - result[result.Count - 1].FireTime < gap)
                    continue;

                result.Add(candidate);
            }
        }

        // Which wording of a reminder to use on the day it fires.
        public int Variant(PlannedReminder reminder)
        {
            return reminder.FireTime.DayOfYear % _config.TextVariants;
        }

        private bool IsQuiet(DateTime time)
        {
            return time.Hour >= _config.QuietStartHour || time.Hour < _config.QuietEndHour;
        }

        private DateTime OutOfQuietHours(DateTime time)
        {
            if (!IsQuiet(time))
                return time;

            var morning = time.Date.AddHours(_config.QuietEndHour);
            return time.Hour >= _config.QuietStartHour ? morning.AddDays(1) : morning;
        }
    }
}
