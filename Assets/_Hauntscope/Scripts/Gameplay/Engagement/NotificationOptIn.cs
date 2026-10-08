using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Engagement
{
    // The player's answer about notifications: the game's own card first (never the bare system dialog), then the
    // Android permission. Both halves must say yes before anything is scheduled or subscribed.
    public sealed class NotificationOptIn
    {
        private readonly ILocalNotifications _notifications;
        private readonly GameSettings _settings;
        private readonly SettingsRepository _repository;
        private readonly PlayerProgress _progress;
        private readonly NotificationConfig _config;

        public NotificationOptIn(ILocalNotifications notifications, GameSettings settings, SettingsRepository repository,
            PlayerProgress progress, NotificationConfig config)
        {
            _notifications = notifications;
            _settings = settings;
            _repository = repository;
            _progress = progress;
            _config = config;
        }

        public event Action Changed;

        public bool IsActive => _settings.Notifications.Value && _notifications.IsAllowed;

        public bool IsPrompted => _settings.NotificationsPrompted;

        // The card shows after the first catch, once, and only where the system dialog can still appear.
        public bool NeedsCard => IsDue && !_notifications.IsAllowed && _notifications.CanAsk;

        private bool IsDue => !_settings.NotificationsPrompted && _progress.TotalCaptures >= _config.PromptAfterCaptures;

        // Below Android 13 nothing needs asking, and after "don't ask again" asking is pointless: the card is skipped.
        public void SkipCardIfPointless()
        {
            if (IsDue && (_notifications.IsAllowed || !_notifications.CanAsk))
                Answer(_settings.Notifications.Value);
        }

        public async UniTask AcceptAsync(CancellationToken cancellationToken)
        {
            Answer(true);
            if (!_notifications.IsAllowed)
                await _notifications.RequestPermissionAsync(cancellationToken);
            Changed?.Invoke();
        }

        public void Decline()
        {
            Answer(_settings.Notifications.Value);
        }

        // The settings switch: turning it on asks the system again, or sends the player to the app settings when
        // Android no longer shows the dialog.
        public async UniTask SetEnabledAsync(bool enabled, CancellationToken cancellationToken)
        {
            Answer(enabled);
            if (enabled && !_notifications.IsAllowed)
            {
                if (_notifications.CanAsk)
                    await _notifications.RequestPermissionAsync(cancellationToken);
                else
                    _notifications.OpenSettings();
            }

            Changed?.Invoke();
        }

        // Back from the system settings: the permission may have changed there.
        public void Refresh()
        {
            Changed?.Invoke();
        }

        private void Answer(bool enabled)
        {
            _settings.MarkNotificationsPrompted();
            _settings.SetNotifications(enabled);
            _repository.Save(_settings);
            Changed?.Invoke();
        }
    }
}
