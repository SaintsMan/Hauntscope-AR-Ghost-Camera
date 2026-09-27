using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Photo;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // The shutter is heard and felt at the press, before the picture is even saved: in a hunt and in a prank alike.
    public sealed class PhotoFeedback : IStartable, IDisposable
    {
        private readonly IReadOnlyList<IShutter> _shutters;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly AudioConfig _audio;

        public PhotoFeedback(IReadOnlyList<IShutter> shutters, ISfxPlayer sfx, IHaptics haptics, AudioConfig audio)
        {
            _shutters = shutters;
            _sfx = sfx;
            _haptics = haptics;
            _audio = audio;
        }

        public void Start()
        {
            foreach (var shutter in _shutters)
                shutter.ShutterReleased += OnShutterReleased;
        }

        public void Dispose()
        {
            foreach (var shutter in _shutters)
                shutter.ShutterReleased -= OnShutterReleased;
        }

        private void OnShutterReleased()
        {
            _sfx.Play2D(_audio.PhotoShutter, _audio.ShutterVolume, 1f);
            _haptics.Play(HapticStrength.Medium);
        }
    }
}
