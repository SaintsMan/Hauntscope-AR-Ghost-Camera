#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Unity.Notifications.Android;
using UnityEngine;

namespace Hauntscope.Infrastructure.Notifications
{
    // Adapter over Unity Mobile Notifications. The icons are registered in Project Settings → Mobile Notifications by
    // NotificationIconGenerator.
    public sealed class AndroidLocalNotifications : ILocalNotifications
    {
        private const string SmallIcon = "hauntscope_small";
        private const string LargeIcon = "hauntscope_large";

        private readonly NotificationConfig _config;

        public AndroidLocalNotifications(NotificationConfig config)
        {
            _config = config;
        }

        public bool IsAllowed => AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.Allowed;

        // Android shows its dialog until the second refusal; after that only the rationale flag tells the two apart.
        public bool CanAsk
        {
            get
            {
                var status = AndroidNotificationCenter.UserPermissionToPost;
                return status == PermissionStatus.NotRequested
                    || (status == PermissionStatus.Denied && AndroidNotificationCenter.ShouldShowPermissionToPostRationale);
            }
        }

        public async UniTask<bool> RequestPermissionAsync(CancellationToken cancellationToken)
        {
            var request = new PermissionRequest();
            await UniTask.WaitWhile(() => request.Status == PermissionStatus.RequestPending, cancellationToken: cancellationToken);
            return request.Status == PermissionStatus.Allowed;
        }

        public void OpenSettings()
        {
            AndroidNotificationCenter.OpenNotificationSettings();
        }

        public void RegisterChannel(string id, string name, string description)
        {
            AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel(id, name, description, Importance.Default));
        }

        public void Schedule(LocalNotification notification)
        {
            var android = new AndroidNotification(notification.Title, notification.Body, notification.FireTime)
            {
                SmallIcon = SmallIcon,
                LargeIcon = LargeIcon,
                Color = _config.Accent,
                IntentData = notification.Kind,
                ShouldAutoCancel = true,
                Style = NotificationStyle.BigTextStyle
            };
            AndroidNotificationCenter.SendNotification(android, notification.ChannelId);
        }

        public void CancelAll()
        {
            try
            {
                AndroidNotificationCenter.CancelAllNotifications();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: notifications unavailable ({exception.Message}).");
            }
        }
    }
}
#endif
