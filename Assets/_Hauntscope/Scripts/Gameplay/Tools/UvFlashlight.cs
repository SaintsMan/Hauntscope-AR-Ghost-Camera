using System;
using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // GDD 5.33.4: the ghost leaves ectoplasm wherever it goes, always; the UV beam only makes it visible. So switching
    // UV on shows the last TrailLifetime seconds of its path, where it jumped and where it hid, inside the beam's cone.
    public sealed class UvFlashlight : IViewMode, IRevealingLight, IDisposable
    {
        private const float FloorLift = 0.01f;

        private readonly HuntSession _session;
        private readonly ICameraPose _camera;
        private readonly IPlaneProvider _planes;
        private readonly UvFlashlightConfig _config;
        private readonly PlayerProgress _progress;
        private readonly GhostTrail _trail;
        private readonly float[] _visibility;
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();

        private Ghost _ghost;
        private float _time;

        public UvFlashlight(HuntSession session, ICameraPose camera, IPlaneProvider planes, UvFlashlightConfig config, PlayerProgress progress)
        {
            _session = session;
            _camera = camera;
            _planes = planes;
            _config = config;
            _progress = progress;
            _trail = new GhostTrail(config.TrailCapacity, config.TrailSpacing);
            _visibility = new float[config.TrailCapacity];
        }

        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        public float DrainPerSecond => _config.Drain;

        public bool IsUnlocked => _progress.TotalCaptures >= _config.UnlockCaptures;

        public string LabelKey => _config.LabelKey;

        public Material Filter => _config.Filter;

        public AudioClip OnClip => _config.OnClip;

        public float OnVolume => _config.OnVolume;

        public GhostTrail Trail => _trail;

        public void Activate()
        {
            if (IsUnlocked)
                _isActive.Value = true;
        }

        public void Deactivate()
        {
            _isActive.Value = false;
        }

        // How brightly the beam shows the mark in this trail slot right now: 0 outside the cone or when off.
        public float VisibilityOf(int index)
        {
            return _visibility[index];
        }

        public bool Lights(Vector3 position)
        {
            return _isActive.Value && ConeLight(position) > 0f;
        }

        public void Tick(float deltaTime)
        {
            _time += deltaTime;
            Follow(_session.Ghost.Value);
            if (_ghost != null && !_ghost.IsCaptured && !_ghost.IsEscaped)
                _trail.Follow(OnFloor(_ghost.Position), _time);

            for (var i = 0; i < _visibility.Length; i++)
                _visibility[i] = _isActive.Value ? VisibilityOf(_trail[i]) : 0f;
        }

        public void Dispose()
        {
            Follow(null);
        }

        // A new ghost (the next round of a night shift) starts a fresh trail.
        private void Follow(Ghost ghost)
        {
            if (ghost == _ghost)
                return;

            if (_ghost != null)
            {
                _ghost.Teleported -= OnJumped;
                _ghost.Dashed -= OnJumped;
                _ghost.HideStarted -= OnHid;
            }

            _ghost = ghost;
            _trail.Clear();
            if (_ghost == null)
                return;

            _ghost.Teleported += OnJumped;
            _ghost.Dashed += OnJumped;
            _ghost.HideStarted += OnHid;
        }

        private void OnJumped(Vector3 from, Vector3 to)
        {
            _trail.Mark(OnFloor(from), TrailMarkKind.Splash, _time);
            _trail.Mark(OnFloor(to), TrailMarkKind.Splash, _time);
            _trail.Restart(OnFloor(to));
        }

        private void OnHid(Vector3 spot)
        {
            _trail.Mark(OnFloor(spot), TrailMarkKind.Handprint, _time);
        }

        private float VisibilityOf(TrailMark mark)
        {
            if (mark.IsEmpty)
                return 0f;

            var age = _time - mark.Time;
            if (age >= _config.TrailLifetime)
                return 0f;

            var fresh = 1f - Mathf.InverseLerp(_config.TrailLifetime * _config.FadeStart, _config.TrailLifetime, age);
            return fresh * ConeLight(mark.Position);
        }

        private float ConeLight(Vector3 position)
        {
            var toPoint = position - _camera.Position;
            if (toPoint.magnitude > _config.Range)
                return 0f;

            var angle = Vector3.Angle(_camera.Forward, toPoint);
            return 1f - Mathf.InverseLerp(_config.ConeAngle - _config.ConeSoftness, _config.ConeAngle, angle);
        }

        private Vector3 OnFloor(Vector3 position)
        {
            return new Vector3(position.x, _planes.FloorHeight + FloorLift, position.z);
        }
    }
}
