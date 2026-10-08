using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Iap;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Engagement
{
    // Leaves the planned reminders with the system when the game goes to the background and takes them back when the
    // player returns: a reminder only makes sense while the player is away.
    public sealed class Reminders : IStartable, IDisposable
    {
        private const string ChannelNameKey = "notify.channel.reminders";
        private const string ChannelDescriptionKey = "notify.channel.reminders.description";
        private const string NewsNameKey = "notify.channel.news";
        private const string NewsDescriptionKey = "notify.channel.news.description";

        private readonly ILocalNotifications _notifications;
        private readonly IApplicationLifecycle _lifecycle;
        private readonly ILocalizationService _localization;
        private readonly IClock _clock;
        private readonly NotificationOptIn _optIn;
        private readonly LoginCalendar _calendar;
        private readonly StarterOffer _starter;
        private readonly ReminderPlanner _planner;
        private readonly NotificationConfig _config;
        private readonly List<PlannedReminder> _plan = new List<PlannedReminder>();

        public Reminders(ILocalNotifications notifications, IApplicationLifecycle lifecycle, ILocalizationService localization,
            IClock clock, NotificationOptIn optIn, LoginCalendar calendar, StarterOffer starter, ReminderPlanner planner,
            NotificationConfig config)
        {
            _notifications = notifications;
            _lifecycle = lifecycle;
            _localization = localization;
            _clock = clock;
            _optIn = optIn;
            _calendar = calendar;
            _starter = starter;
            _planner = planner;
            _config = config;
        }

        public void Start()
        {
            RegisterChannels();
            _notifications.CancelAll();
            _lifecycle.Paused += OnPaused;
            _lifecycle.Resumed += OnResumed;
            _localization.Changed += RegisterChannels;
        }

        public void Dispose()
        {
            _lifecycle.Paused -= OnPaused;
            _lifecycle.Resumed -= OnResumed;
            _localization.Changed -= RegisterChannels;
        }

        // Channel names show in the system settings, in the language of the last launch.
        private void RegisterChannels()
        {
            _notifications.RegisterChannel(_config.ReminderChannel, Text(ChannelNameKey), Text(ChannelDescriptionKey));
            _notifications.RegisterChannel(_config.NewsChannel, Text(NewsNameKey), Text(NewsDescriptionKey));
        }

        private void OnPaused()
        {
            _notifications.CancelAll();
            if (!_optIn.IsActive)
                return;

            _starter.Refresh();
            DateTime? offerEnds = _starter.IsActive ? _clock.LocalNow + _starter.Remaining : (DateTime?)null;
            _planner.Plan(new ReminderFacts(_clock.LocalNow, _calendar.IsClaimedToday, offerEnds), _plan);
            foreach (var reminder in _plan)
            {
                var stem = $"notify.{reminder.Kind}.{_planner.Variant(reminder)}";
                _notifications.Schedule(new LocalNotification(_config.ReminderChannel, reminder.Kind, Text(stem + ".title"),
                    Text(stem + ".body"), reminder.FireTime));
            }
        }

        private void OnResumed()
        {
            _notifications.CancelAll();
            _optIn.Refresh();
        }

        private string Text(string key)
        {
            return _localization.Get(LocalizationTable.Ui, key);
        }
    }
}
