using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    public sealed class HuntAmbience : IStartable, IDisposable
    {
        private readonly ISfxPlayer _sfx;
        private readonly AudioConfig _config;
        private readonly WitchingHour _witchingHour;
        private ISfxLoop _drone;
        private ISfxLoop _static;

        public HuntAmbience(ISfxPlayer sfx, AudioConfig config, WitchingHour witchingHour)
        {
            _sfx = sfx;
            _config = config;
            _witchingHour = witchingHour;
        }

        public void Start()
        {
            _drone = _sfx.PlayLoop(_config.AmbientDrone, _config.AmbientDroneVolume, false);
            // The room hums lower after dark (GDD 5.28).
            if (_witchingHour.IsActive)
                _drone?.SetPitch(_config.WitchingHourDronePitch);
            _static = _sfx.PlayLoop(_config.AmbientStatic, _config.AmbientStaticVolume, false);
        }

        public void Dispose()
        {
            _drone?.Stop();
            _static?.Stop();
        }
    }
}
