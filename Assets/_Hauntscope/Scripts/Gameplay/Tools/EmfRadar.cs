using System.Collections.Generic;
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

        private readonly List<Vector3> _single = new List<Vector3>(1);
        private readonly List<EmfSignal> _signals = new List<EmfSignal>(8);

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

        // Horizontal angle from where the camera looks to the source, in degrees; positive is to the right.
        public float Bearing { get; private set; }

        // Every source the radar hears right now (the jack-o'-lantern's pumpkins are many); the meter reads the strongest.
        public IReadOnlyList<EmfSignal> Signals => _signals;

        public void Tick(float deltaTime, Vector3 sourcePosition, float range)
        {
            _single.Clear();
            _single.Add(sourcePosition);
            Tick(deltaTime, _single, range);
        }

        public void Tick(float deltaTime, IReadOnlyList<Vector3> sources, float range)
        {
            // Noise is resampled on an interval so the level doesn't flicker every frame at a bucket edge.
            _timeUntilNoise -= deltaTime;
            if (_timeUntilNoise <= 0f)
            {
                _noise = _random.Range(-_config.NoiseAmplitude, _config.NoiseAmplitude);
                _timeUntilNoise = _config.NoiseInterval;
            }

            _signals.Clear();
            var strongest = 0f;
            var bearing = 0f;
            for (var i = 0; i < sources.Count; i++)
            {
                var toSource = sources[i] - _camera.Position;
                var distance = toSource.magnitude;
                var proximity = range > 0f ? Mathf.Clamp01(1f - distance / range) : 0f;
                var direction = distance > 0f ? toSource / distance : _camera.Forward;
                var facing = Mathf.Clamp01(Vector3.Dot(_camera.Forward, direction) * 0.5f + 0.5f);
                var sourceBearing = Vector3.SignedAngle(Flat(_camera.Forward), Flat(toSource), Vector3.up);
                var signal = proximity > 0f ? proximity * Mathf.Lerp(_config.FacingMinFactor, 1f, facing) + _noise : 0f;
                if (proximity > 0f)
                    _signals.Add(new EmfSignal(sourceBearing, proximity));

                if (i == 0 || signal > strongest)
                {
                    strongest = signal;
                    bearing = sourceBearing;
                }
            }

            Bearing = bearing;
            Value = Mathf.Clamp01(strongest);
            _level.Value = Mathf.Min(Mathf.FloorToInt(Value * (_config.MaxLevel + 1)), _config.MaxLevel);
        }

        public void Reset()
        {
            Value = 0f;
            Bearing = 0f;
            _signals.Clear();
            _level.Value = 0;
            _timeUntilNoise = 0f;
        }

        private static Vector3 Flat(Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }
    }
}
