using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    public sealed class HuntAmbience : IStartable, IDisposable
    {
        private readonly ISfxPlayer _sfx;
        private readonly AudioConfig _config;
        private ISfxLoop _drone;
        private ISfxLoop _static;

        public HuntAmbience(ISfxPlayer sfx, AudioConfig config)
        {
            _sfx = sfx;
            _config = config;
        }

        public void Start()
        {
            _drone = _sfx.PlayLoop(_config.AmbientDrone, _config.AmbientDroneVolume, false);
            _static = _sfx.PlayLoop(_config.AmbientStatic, _config.AmbientStaticVolume, false);
        }

        public void Dispose()
        {
            _drone?.Stop();
            _static?.Stop();
        }
    }
}
