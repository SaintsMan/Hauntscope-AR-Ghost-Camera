using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Engagement;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // Whether the messages left for players bring them back: a push tapped open (or arriving mid-game), a reminder
    // tapped open, and the device joining the push topics. Names and parameters are the ones every Pavko game shares.
    public sealed class NotificationAnalytics : IStartable, IDisposable
    {
        private readonly IAnalyticsService _analytics;
        private readonly IPushMessaging _push;
        private readonly PushTopics _topics;
        private readonly ILocalNotifications _notifications;
        private readonly IApplicationLifecycle _lifecycle;

        public NotificationAnalytics(IAnalyticsService analytics, IPushMessaging push, PushTopics topics, ILocalNotifications notifications,
            IApplicationLifecycle lifecycle)
        {
            _analytics = analytics;
            _push = push;
            _topics = topics;
            _notifications = notifications;
            _lifecycle = lifecycle;
        }

        public void Start()
        {
            _push.Received += OnPushReceived;
            _topics.Subscribed += OnSubscribed;
            _lifecycle.Resumed += OnResumed;
            OnResumed();
        }

        public void Dispose()
        {
            _push.Received -= OnPushReceived;
            _topics.Subscribed -= OnSubscribed;
            _lifecycle.Resumed -= OnResumed;
        }

        private void OnPushReceived(PushMessage message)
        {
            _analytics.Log(message.Opened ? AnalyticsNames.PushOpen : AnalyticsNames.PushReceived,
                AnalyticsParameter.Of(AnalyticsNames.Topic, message.Topic));
        }

        private void OnSubscribed(string language)
        {
            _analytics.Log(AnalyticsNames.PushSubscribed, AnalyticsParameter.Of(AnalyticsNames.Language, language));
        }

        // A tap on a reminder either starts the game or brings it back from the background.
        private void OnResumed()
        {
            if (_notifications.TryTakeOpenedKind(out var kind))
                _analytics.Log(AnalyticsNames.ReminderOpen, AnalyticsParameter.Of(AnalyticsNames.Kind, kind));
        }
    }
}
