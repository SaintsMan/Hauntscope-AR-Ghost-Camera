using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Infrastructure.Haptics
{
    // Decorator: the platform haptics stay unaware of the player's "Vibration" setting.
    public sealed class SettingsAwareHaptics : IHaptics
    {
        private readonly IHaptics _inner;
        private readonly GameSettings _settings;

        public SettingsAwareHaptics(IHaptics inner, GameSettings settings)
        {
            _inner = inner;
            _settings = settings;
        }

        public void Play(HapticStrength strength)
        {
            if (_settings.Vibration.Value)
                _inner.Play(strength);
        }
    }
}
