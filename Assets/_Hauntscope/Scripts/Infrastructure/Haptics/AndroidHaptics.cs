using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using UnityEngine;
using UnityEngine.Android;

namespace Hauntscope.Infrastructure.Haptics
{
    public sealed class AndroidHaptics : IHaptics, IDisposable
    {
        private const int VibrationEffectMinSdk = 26;

        private readonly HapticsConfig _config;
        private readonly AndroidJavaObject _vibrator;
        private readonly AndroidJavaObject _lightEffect;
        private readonly AndroidJavaObject _mediumEffect;
        private readonly AndroidJavaObject _heavyEffect;

        public AndroidHaptics(HapticsConfig config)
        {
            _config = config;
            _vibrator = AndroidApplication.currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            if (version.GetStatic<int>("SDK_INT") < VibrationEffectMinSdk)
                return;

            // VibrationEffect objects are immutable, so they are created once instead of on every pulse.
            using var effects = new AndroidJavaClass("android.os.VibrationEffect");
            _lightEffect = CreateEffect(effects, config.LightDurationMs, config.LightAmplitude);
            _mediumEffect = CreateEffect(effects, config.MediumDurationMs, config.MediumAmplitude);
            _heavyEffect = CreateEffect(effects, config.HeavyDurationMs, config.HeavyAmplitude);
        }

        public void Play(HapticStrength strength)
        {
            if (_vibrator == null)
                return;

            var effect = GetEffect(strength);
            if (effect != null)
                _vibrator.Call("vibrate", effect);
            else
                _vibrator.Call("vibrate", (long)GetDurationMs(strength));
        }

        public void Dispose()
        {
            _lightEffect?.Dispose();
            _mediumEffect?.Dispose();
            _heavyEffect?.Dispose();
            _vibrator?.Dispose();
        }

        private AndroidJavaObject GetEffect(HapticStrength strength)
        {
            return strength switch
            {
                HapticStrength.Light => _lightEffect,
                HapticStrength.Medium => _mediumEffect,
                _ => _heavyEffect
            };
        }

        private int GetDurationMs(HapticStrength strength)
        {
            return strength switch
            {
                HapticStrength.Light => _config.LightDurationMs,
                HapticStrength.Medium => _config.MediumDurationMs,
                _ => _config.HeavyDurationMs
            };
        }

        private static AndroidJavaObject CreateEffect(AndroidJavaClass effects, int durationMs, int amplitude)
        {
            return effects.CallStatic<AndroidJavaObject>("createOneShot", (long)durationMs, amplitude);
        }
    }
}
