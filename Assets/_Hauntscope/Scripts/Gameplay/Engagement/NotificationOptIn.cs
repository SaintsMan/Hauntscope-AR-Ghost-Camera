using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Onboarding;
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
        private readonly FeatureUnlocks _features;

        public NotificationOptIn(ILocalNotifications notifications, GameSettings settings, SettingsRepository repository,
            FeatureUnlocks features)
        {
            _notifications = notifications;
            _settings = settings;
            _repository = repository;
            _features = features;
        }

        public event Action Changed;

        public bool IsActive => _settings.Notifications.Value && _notifications.IsAllowed;

        public bool IsPrompted => _settings.NotificationsPrompted;

        // The card shows once the first hunt is behind the player, whatever its result, and only where the system dialog
        // can still appear.
        public bool NeedsCard => IsDue && !_notifications.IsAllowed && _notifications.CanAsk;

        private bool IsDue => !_settings.NotificationsPrompted && _features.IsRuleMet(Feature.Notifications);

        // Below Android 13 nothing needs asking, and after "don't ask again" asking is pointless: no card, and the
        // answer is given at once, so a player who leaves before ever seeing a card is still on the push topics.
        public void SkipCardIfPointless()
        {
            if (!_settings.NotificationsPrompted && (_notifications.IsAllowed || !_notifications.CanAsk))
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
