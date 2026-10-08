using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Progress
{
    public sealed class GameSettings
    {
        private readonly ObservableValue<bool> _sound;
        private readonly ObservableValue<bool> _music;
        private readonly ObservableValue<bool> _vibration;
        private readonly ObservableValue<bool> _jumpScares;
        private readonly ObservableValue<bool> _occlusion;
        private readonly ObservableValue<string> _language;
        private readonly ObservableValue<HuntEnvironment> _environment;
        private readonly ObservableValue<bool> _notifications;

        public GameSettings()
            : this(true, true, true, true, string.Empty)
        {
        }

        public GameSettings(bool sound, bool vibration, bool jumpScares, bool occlusion, string language,
            HuntEnvironment environment = HuntEnvironment.Ar, bool music = true, bool notifications = true,
            bool notificationsPrompted = false, string pushLanguage = "")
        {
            _notifications = new ObservableValue<bool>(notifications);
            NotificationsPrompted = notificationsPrompted;
            PushLanguage = pushLanguage ?? string.Empty;
            _music = new ObservableValue<bool>(music);
            _environment = new ObservableValue<HuntEnvironment>(environment);
            _sound = new ObservableValue<bool>(sound);
            _vibration = new ObservableValue<bool>(vibration);
            _jumpScares = new ObservableValue<bool>(jumpScares);
            _occlusion = new ObservableValue<bool>(occlusion);
            _language = new ObservableValue<string>(language ?? string.Empty);
        }

        public IReadOnlyObservableValue<bool> Sound => _sound;

        // The soundtrack only; SFX follow Sound.
        public IReadOnlyObservableValue<bool> Music => _music;

        public IReadOnlyObservableValue<bool> Vibration => _vibration;

        public IReadOnlyObservableValue<bool> JumpScares => _jumpScares;

        public IReadOnlyObservableValue<bool> Occlusion => _occlusion;

        // Empty means "follow the system language".
        public IReadOnlyObservableValue<string> Language => _language;

        // Where START HUNT goes: through the camera, or into the Virtual Room without one.
        public IReadOnlyObservableValue<HuntEnvironment> Environment => _environment;

        // The player's wish; the system permission is the other half (NotificationOptIn).
        public IReadOnlyObservableValue<bool> Notifications => _notifications;

        // The opt-in card has been answered (or skipped where Android needs no permission).
        public bool NotificationsPrompted { get; private set; }

        // The language whose push topic this device is subscribed to; empty when it is subscribed to none.
        public string PushLanguage { get; private set; }

        public void SetNotifications(bool enabled)
        {
            _notifications.Value = enabled;
        }

        public void MarkNotificationsPrompted()
        {
            NotificationsPrompted = true;
        }

        public void SetPushLanguage(string languageCode)
        {
            PushLanguage = languageCode ?? string.Empty;
        }

        public void SetSound(bool enabled)
        {
            _sound.Value = enabled;
        }

        public void SetMusic(bool enabled)
        {
            _music.Value = enabled;
        }

        public void SetVibration(bool enabled)
        {
            _vibration.Value = enabled;
        }

        public void SetJumpScares(bool enabled)
        {
            _jumpScares.Value = enabled;
        }

        public void SetOcclusion(bool enabled)
        {
            _occlusion.Value = enabled;
        }

        public void SetEnvironment(HuntEnvironment environment)
        {
            _environment.Value = environment;
        }

        public void SetLanguage(string languageCode)
        {
            _language.Value = languageCode ?? string.Empty;
        }
    }
}
