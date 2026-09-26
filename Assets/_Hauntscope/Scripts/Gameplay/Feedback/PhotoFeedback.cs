using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Photo;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // The shutter is heard and felt at the press, before the picture is even saved.
    public sealed class PhotoFeedback : IStartable, IDisposable
    {
        private readonly SpiritCamera _camera;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly AudioConfig _audio;

        public PhotoFeedback(SpiritCamera camera, ISfxPlayer sfx, IHaptics haptics, AudioConfig audio)
        {
            _camera = camera;
            _sfx = sfx;
            _haptics = haptics;
            _audio = audio;
        }

        public void Start()
        {
            _camera.ShutterReleased += OnShutterReleased;
        }

        public void Dispose()
        {
            _camera.ShutterReleased -= OnShutterReleased;
        }

        private void OnShutterReleased()
        {
            _sfx.Play2D(_audio.PhotoShutter, _audio.ShutterVolume, 1f);
            _haptics.Play(HapticStrength.Medium);
        }
    }
}
