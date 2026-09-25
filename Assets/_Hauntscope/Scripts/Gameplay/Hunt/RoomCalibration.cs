using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class RoomCalibration
    {
        private readonly IPlaneProvider _planes;
        private readonly RoomConfig _config;
        private readonly ObservableValue<float> _progress = new ObservableValue<float>();

        public RoomCalibration(IPlaneProvider planes, RoomConfig config)
        {
            _planes = planes;
            _config = config;
        }

        public IReadOnlyObservableValue<float> Progress => _progress;

        public bool IsComplete => _progress.Value >= 1f;

        public void Refresh()
        {
            _progress.Value = _config.CalibrationArea > 0f
                ? Mathf.Clamp01(_planes.HorizontalArea / _config.CalibrationArea)
                : 1f;
        }
    }
}
