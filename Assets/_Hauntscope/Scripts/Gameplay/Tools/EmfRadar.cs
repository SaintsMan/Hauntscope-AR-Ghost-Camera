using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    public sealed class EmfRadar
    {
        private readonly ICameraPose _camera;
        private readonly IRandom _random;
        private readonly EmfConfig _config;
        private readonly ObservableValue<int> _level = new ObservableValue<int>();

        private float _noise;
        private float _timeUntilNoise;

        public EmfRadar(ICameraPose camera, IRandom random, EmfConfig config)
        {
            _camera = camera;
            _random = random;
            _config = config;
        }

        public IReadOnlyObservableValue<int> Level => _level;

        public float Value { get; private set; }

        public void Tick(float deltaTime, Vector3 sourcePosition, float range)
        {
            // Noise is resampled on an interval so the level doesn't flicker every frame at a bucket edge.
            _timeUntilNoise -= deltaTime;
            if (_timeUntilNoise <= 0f)
            {
                _noise = _random.Range(-_config.NoiseAmplitude, _config.NoiseAmplitude);
                _timeUntilNoise = _config.NoiseInterval;
            }

            var toSource = sourcePosition - _camera.Position;
            var distance = toSource.magnitude;
            var proximity = range > 0f ? Mathf.Clamp01(1f - distance / range) : 0f;
            var direction = distance > 0f ? toSource / distance : _camera.Forward;
            var facing = Mathf.Clamp01(Vector3.Dot(_camera.Forward, direction) * 0.5f + 0.5f);
            var signal = proximity > 0f ? proximity * Mathf.Lerp(_config.FacingMinFactor, 1f, facing) + _noise : 0f;

            Value = Mathf.Clamp01(signal);
            _level.Value = Mathf.Min(Mathf.FloorToInt(Value * (_config.MaxLevel + 1)), _config.MaxLevel);
        }

        public void Reset()
        {
            Value = 0f;
            _level.Value = 0;
            _timeUntilNoise = 0f;
        }
    }
}
