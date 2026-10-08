using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ReminderPlannerTests
    {
        private static readonly DateTime Day = new DateTime(2026, 10, 20);

        private ReminderPlanner _planner;
        private List<PlannedReminder> _plan;

        [SetUp]
        public void SetUp()
        {
            var config = new NotificationConfig(quietStartHour: 21, quietEndHour: 10, eveningHour: 19, morningHour: 11,
                returnDays: new[] { 3, 7 }, minGapHours: 3f, minLeadMinutes: 30f, offerWarningHours: 3f);
            _planner = new ReminderPlanner(config);
            _plan = new List<PlannedReminder>();
        }

        [Test]
        public void Plan_RationUnclaimedInTheAfternoon_RemindsThisEvening()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(14), false, null), _plan);

            Assert.AreEqual(Day.AddHours(19), Find(ReminderPlanner.Ration).FireTime);
        }

        [Test]
        public void Plan_RationAlreadyClaimed_RemindsTomorrowMorning()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(14), true, null), _plan);

            Assert.AreEqual(Day.AddDays(1).AddHours(11), Find(ReminderPlanner.Ration).FireTime);
        }

        [Test]
        public void Plan_RationUnclaimedLateInTheEvening_RemindsTomorrowMorning()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(20), false, null), _plan);

            Assert.AreEqual(Day.AddDays(1).AddHours(11), Find(ReminderPlanner.Ration).FireTime);
        }

        [Test]
        public void Plan_ReturnFallingAtNight_WaitsForTheMorning()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(23.5), true, null), _plan);

            Assert.AreEqual(Day.AddDays(4).AddHours(10), Find(ReminderPlanner.ReturnPrefix + 3).FireTime);
            Assert.AreEqual(Day.AddDays(8).AddHours(10), Find(ReminderPlanner.ReturnPrefix + 7).FireTime);
        }

        [Test]
        public void Plan_OfferEndingInTheEvening_WarnsHoursBefore()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(10), true, Day.AddHours(18)), _plan);

            Assert.AreEqual(Day.AddHours(15), Find(ReminderPlanner.Offer).FireTime);
        }

        [Test]
        public void Plan_OfferWarningAtNight_IsDropped()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(12), true, Day.AddDays(1).AddHours(2)), _plan);

            Assert.IsFalse(Contains(ReminderPlanner.Offer));
        }

        [Test]
        public void Plan_TwoRemindersCloseTogether_KeepsTheEarlierOne()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(12), false, Day.AddHours(21)), _plan);

            Assert.IsTrue(Contains(ReminderPlanner.Offer));
            Assert.IsFalse(Contains(ReminderPlanner.Ration));
        }

        [Test]
        public void Plan_Always_KeepsTimeOrderAndGaps()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(9), false, Day.AddHours(16)), _plan);

            for (var i = 1; i < _plan.Count; i++)
                Assert.GreaterOrEqual((_plan[i].FireTime - _plan[i - 1].FireTime).TotalHours, 3d);
        }

        [Test]
        public void Plan_UnheardTape_AnnouncesItTomorrowEvening()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(10), true, null, 1), _plan);

            Assert.AreEqual(Day.AddDays(1).AddHours(18), Find(ReminderPlanner.Tape).FireTime);
        }

        [Test]
        public void Plan_FewCatches_LeavesOutTheWitchingHour()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(10), true, null, 0, 4), _plan);

            Assert.IsFalse(Contains(ReminderPlanner.Witching));
        }

        [Test]
        public void Plan_SeasonedAgent_WarnsOfTheWitchingHourBeforeTheQuietHours()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(10), true, null, 0, 5), _plan);

            Assert.AreEqual(Day.AddDays(2).AddHours(20), Find(ReminderPlanner.Witching).FireTime);
        }

        [Test]
        public void Variant_AnyDay_StaysWithinTheWordings()
        {
            _planner.Plan(new ReminderFacts(Day.AddHours(14), false, null), _plan);

            foreach (var reminder in _plan)
                Assert.That(_planner.Variant(reminder), Is.InRange(0, 1));
        }

        private PlannedReminder Find(string kind)
        {
            foreach (var reminder in _plan)
            {
                if (reminder.Kind == kind)
                    return reminder;
            }

            Assert.Fail($"No {kind} reminder in the plan.");
            return default;
        }

        private bool Contains(string kind)
        {
            foreach (var reminder in _plan)
            {
                if (reminder.Kind == kind)
                    return true;
            }

            return false;
        }
    }
}
