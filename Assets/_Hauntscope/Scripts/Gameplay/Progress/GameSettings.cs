using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Progress
{
    public sealed class GameSettings
    {
        private readonly ObservableValue<bool> _sound;
        private readonly ObservableValue<bool> _vibration;
        private readonly ObservableValue<bool> _jumpScares;
        private readonly ObservableValue<bool> _occlusion;
        private readonly ObservableValue<string> _language;
        private readonly ObservableValue<HuntEnvironment> _environment;

        public GameSettings()
            : this(true, true, true, true, string.Empty)
        {
        }

        public GameSettings(bool sound, bool vibration, bool jumpScares, bool occlusion, string language,
            HuntEnvironment environment = HuntEnvironment.Ar)
        {
            _environment = new ObservableValue<HuntEnvironment>(environment);
            _sound = new ObservableValue<bool>(sound);
            _vibration = new ObservableValue<bool>(vibration);
            _jumpScares = new ObservableValue<bool>(jumpScares);
            _occlusion = new ObservableValue<bool>(occlusion);
            _language = new ObservableValue<string>(language ?? string.Empty);
        }

        public IReadOnlyObservableValue<bool> Sound => _sound;

        public IReadOnlyObservableValue<bool> Vibration => _vibration;

        public IReadOnlyObservableValue<bool> JumpScares => _jumpScares;

        public IReadOnlyObservableValue<bool> Occlusion => _occlusion;

        // Empty means "follow the system language".
        public IReadOnlyObservableValue<string> Language => _language;

        // Where START HUNT goes: through the camera, or into the Virtual Room without one.
        public IReadOnlyObservableValue<HuntEnvironment> Environment => _environment;

        public void SetSound(bool enabled)
        {
            _sound.Value = enabled;
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
