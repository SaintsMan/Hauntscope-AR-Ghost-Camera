using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Progress;
using UnityEngine;

namespace Hauntscope.Gameplay.Photo
{
    // GDD 5.33.6: pose a ghost the player has caught in the room and take a photo of it to fool friends with. The ghost
    // follows where the camera points at the floor until it is placed; then drags move it and two fingers turn and size
    // it. The photo goes through the same camcorder frame as a real one, but never into the album: it is not evidence.
    public sealed class PrankPhotoMode : IShutter, IDisposable
    {
        private const float MinDownward = 0.05f;

        private readonly PlayerProgress _progress;
        private readonly GhostConfig _ghosts;
        private readonly ICameraPose _camera;
        private readonly IPlaneProvider _planes;
        private readonly IGhostViewSpawner _spawner;
        private readonly IPhotoCapture _capture;
        private readonly IPhotoStorage _storage;
        private readonly PrankPhotoConfig _config;
        private readonly IClock _clock;
        private readonly List<GhostData> _roster = new List<GhostData>();
        private readonly ObservableValue<PrankPhase> _phase = new ObservableValue<PrankPhase>(PrankPhase.Off);
        private readonly ObservableValue<GhostData> _ghost = new ObservableValue<GhostData>();
        private readonly ObservableValue<bool> _isShooting = new ObservableValue<bool>();

        private IGhostView _view;
        private Vector3 _floorPoint;
        private float _facing;
        private float _turn;
        private float _scale = 1f;
        private string _lastPhoto;

        public PrankPhotoMode(
            PlayerProgress progress,
            GhostConfig ghosts,
            ICameraPose camera,
            IPlaneProvider planes,
            IGhostViewSpawner spawner,
            IPhotoCapture capture,
            IPhotoStorage storage,
            PrankPhotoConfig config,
            IClock clock)
        {
            _progress = progress;
            _ghosts = ghosts;
            _camera = camera;
            _planes = planes;
            _spawner = spawner;
            _capture = capture;
            _storage = storage;
            _config = config;
            _clock = clock;
        }

        public event Action ShutterReleased;

        public IReadOnlyObservableValue<PrankPhase> Phase => _phase;

        public IReadOnlyObservableValue<GhostData> Ghost => _ghost;

        public IReadOnlyObservableValue<bool> IsShooting => _isShooting;

        // Caught ghosts only, in Bestiary order.
        public IReadOnlyList<GhostData> Roster => _roster;

        public bool IsUnlocked => _progress.TotalCaptures >= _config.UnlockCaptures;

        // Where the ghost stands on the floor, which way it faces (degrees) and how big it is.
        public Vector3 FloorPoint => _floorPoint;

        public float Yaw => _facing + _turn;

        public float Scale => _scale;

        public void Begin()
        {
            _roster.Clear();
            foreach (var ghost in _ghosts.Ghosts)
            {
                if (_progress.GetCaptureCount(ghost.Id) > 0)
                    _roster.Add(ghost);
            }

            _phase.Value = PrankPhase.Aiming;
            if (_roster.Count > 0)
                Select(_roster[0]);
        }

        // Another ghost takes the pose from the start: it comes back to the camera's aim, facing the camera.
        public void Select(GhostData ghost)
        {
            if (_phase.Value == PrankPhase.Off || ghost == _ghost.Value || !_roster.Contains(ghost))
                return;

            _view?.Despawn();
            _view = _spawner.Spawn(ghost);
            _view.SetReveal(1f);
            _turn = 0f;
            _scale = 1f;
            _ghost.Value = ghost;
            _phase.Value = PrankPhase.Aiming;
            Aim();
            Apply();
        }

        public void Place()
        {
            if (_phase.Value == PrankPhase.Aiming && _view != null)
                _phase.Value = PrankPhase.Placed;
        }

        public void Lift()
        {
            if (_phase.Value == PrankPhase.Placed)
                _phase.Value = PrankPhase.Aiming;
        }

        // A drag in screen heights: right and up move the ghost right and away, further for a ghost further off.
        public void Move(Vector2 screenDelta)
        {
            if (_phase.Value != PrankPhase.Placed)
                return;

            // In the camera's terms, so pulling the ghost in stops in front of the lens instead of passing behind it.
            var forward = Direction(_camera.Forward);
            var right = new Vector3(forward.z, 0f, -forward.x);
            var offset = Flat(_floorPoint - _camera.Position);
            var gain = offset.magnitude * _config.DragGain;
            var depth = Mathf.Max(Vector3.Dot(offset, forward) + screenDelta.y * gain, _config.MinDistance);
            var side = Vector3.Dot(offset, right) + screenDelta.x * gain;
            _floorPoint = ClampDistance(_camera.Position + forward * depth + right * side);
            Apply();
        }

        public void Turn(float degrees)
        {
            if (_phase.Value != PrankPhase.Placed)
                return;

            _turn += degrees;
            Apply();
        }

        public void Resize(float factor)
        {
            if (_phase.Value != PrankPhase.Placed || factor <= 0f)
                return;

            _scale = Mathf.Clamp(_scale * factor, _config.MinScale, _config.MaxScale);
            Apply();
        }

        public void Tick(float deltaTime)
        {
            if (_view == null)
                return;

            if (_phase.Value == PrankPhase.Aiming)
                Aim();
            Apply();
        }

        // The file of the previous prank shot is dropped: it was never evidence, and only the newest can be shared.
        public async UniTask<PhotoRecord> ShootAsync(CancellationToken cancellationToken)
        {
            var ghost = _ghost.Value;
            if (ghost == null || _view == null || _isShooting.Value)
                return null;

            _isShooting.Value = true;
            ShutterReleased?.Invoke();
            var taken = _clock.UtcNow;
            // No flash, unlike the spirit camera: the ghost is already in full view and must look exactly as posed.
            try
            {
                var fileName = await _capture.CaptureAsync(new PhotoCaption(ghost, 0, taken.ToLocalTime()), cancellationToken);
                ForgetLastPhoto();
                _lastPhoto = fileName;
                return new PhotoRecord(fileName, ghost.Id, 0, new DateTimeOffset(taken).ToUnixTimeSeconds());
            }
            finally
            {
                _isShooting.Value = false;
            }
        }

        public void End()
        {
            _view?.Despawn();
            _view = null;
            _ghost.Value = null;
            _phase.Value = PrankPhase.Off;
            ForgetLastPhoto();
        }

        // Leaving for the menu unloads the scene without leaving the state, so the last prank file goes here.
        public void Dispose()
        {
            ForgetLastPhoto();
        }

        private void ForgetLastPhoto()
        {
            if (_lastPhoto != null)
                _storage.Delete(_lastPhoto);
            _lastPhoto = null;
        }

        // Where the camera's look meets the floor, kept within reach; above the horizon, a step ahead.
        private void Aim()
        {
            var forward = _camera.Forward;
            var point = _camera.Position + Direction(forward) * _config.AimDistance;
            if (forward.y < -MinDownward)
                point = _camera.Position + forward * ((_planes.FloorHeight - _camera.Position.y) / forward.y);

            _floorPoint = ClampDistance(point);
            var toCamera = Flat(_camera.Position - _floorPoint);
            _facing = toCamera.sqrMagnitude > 0f ? Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg : 0f;
        }

        private Vector3 ClampDistance(Vector3 point)
        {
            var offset = Flat(point - _camera.Position);
            var distance = offset.magnitude;
            var clamped = distance > 0f
                ? offset / distance * Mathf.Clamp(distance, _config.MinDistance, _config.MaxDistance)
                : Direction(_camera.Forward) * _config.MinDistance;
            return new Vector3(_camera.Position.x + clamped.x, _planes.FloorHeight, _camera.Position.z + clamped.z);
        }

        // It hovers at its own height above the spot, scaled with its body.
        private void Apply()
        {
            var hover = _ghost.Value.Motion.HoverHeightMin * _scale;
            _view.SetScale(_scale);
            _view.SetPose(_floorPoint + Vector3.up * hover, Quaternion.Euler(0f, Yaw, 0f));
        }

        private static Vector3 Flat(Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }

        // Which way along the floor; straight down counts as ahead.
        private static Vector3 Direction(Vector3 vector)
        {
            var flat = Flat(vector);
            return flat.sqrMagnitude > 0f ? flat.normalized : Vector3.forward;
        }
    }
}
