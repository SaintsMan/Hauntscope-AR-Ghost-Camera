using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    public sealed class EmfFeedback : IStartable, ITickable, IDisposable
    {
        private readonly EmfRadar _radar;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly EmfConfig _config;

        private float _timeUntilBeep;

        public EmfFeedback(EmfRadar radar, ISfxPlayer sfx, IHaptics haptics, EmfConfig config)
        {
            _radar = radar;
            _sfx = sfx;
            _haptics = haptics;
            _config = config;
        }

        public void Start()
        {
            _radar.Level.Changed += OnLevelChanged;
        }

        void ITickable.Tick()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            var level = _radar.Level.Value;
            if (level <= 0)
                return;

            _timeUntilBeep -= deltaTime;
            if (_timeUntilBeep > 0f)
                return;

            Beep(level);
            _timeUntilBeep = Mathf.Lerp(_config.BeepIntervalSlow, _config.BeepIntervalFast, _radar.Value);
        }

        public void Dispose()
        {
            _radar.Level.Changed -= OnLevelChanged;
        }

        private void OnLevelChanged(int level)
        {
            if (level <= 0)
                _timeUntilBeep = 0f;
        }

        private void Beep(int level)
        {
            var intensity = (float)level / _config.MaxLevel;
            _sfx.Play2D(_config.BeepClip, _config.BeepVolume, Mathf.Lerp(_config.BeepPitchMin, _config.BeepPitchMax, intensity));

            if (level >= _config.HapticLevel)
                _haptics.Play(level >= _config.MaxLevel ? HapticStrength.Medium : HapticStrength.Light);
        }
    }
}
