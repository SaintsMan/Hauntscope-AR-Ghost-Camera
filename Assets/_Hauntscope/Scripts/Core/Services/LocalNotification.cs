using System;

namespace Hauntscope.Core.Services
{
    public readonly struct LocalNotification
    {
        public LocalNotification(string channelId, string kind, string title, string body, DateTime fireTime)
        {
            ChannelId = channelId;
            Kind = kind;
            Title = title;
            Body = body;
            FireTime = fireTime;
        }

        public string ChannelId { get; }

        // Which reminder this is; travels with the notification, so a tap can tell what brought the player back.
        public string Kind { get; }

        public string Title { get; }

        public string Body { get; }

        // Local wall-clock time.
        public DateTime FireTime { get; }
    }
}
