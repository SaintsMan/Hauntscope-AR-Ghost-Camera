using System;
using Hauntscope.Gameplay.Progress;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Audio
{
    public sealed class AudioSettingsSync : IInitializable, IDisposable
    {
        private readonly GameSettings _settings;

        public AudioSettingsSync(GameSettings settings)
        {
            _settings = settings;
        }

        public void Initialize()
        {
            _settings.Sound.Changed += Apply;
            Apply(_settings.Sound.Value);
        }

        public void Dispose()
        {
            _settings.Sound.Changed -= Apply;
        }

        private static void Apply(bool enabled)
        {
            AudioListener.volume = enabled ? 1f : 0f;
        }
    }
}
