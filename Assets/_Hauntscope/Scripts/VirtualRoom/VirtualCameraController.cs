using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Hauntscope.VirtualRoom
{
    public sealed class VirtualCameraController : IStartable, ITickable, IDisposable
    {
        private const float MinMoveDistance = 0.01f;

        // Android reports attitude in a right-handed, Z-up frame; this maps it to Unity's left-handed, Y-up frame
        // for a phone held in portrait.
        private static readonly Quaternion DeviceToWorld = Quaternion.Euler(90f, 0f, 0f);

        private readonly Transform _camera;
        private readonly VirtualPointerInput _input;
        private readonly IPlaneProvider _planes;
        private readonly VirtualConfig _config;
        private readonly AttitudeSensor _gyro;

        private Vector3 _target;
        private float _yaw;
        private float _pitch;
        private float _gyroYawOffset;
        private bool _hasGyroReference;

        public VirtualCameraController(Camera camera, VirtualPointerInput input, IPlaneProvider planes, VirtualConfig config)
        {
            _camera = camera.transform;
            _input = input;
            _planes = planes;
            _config = config;
            _gyro = AttitudeSensor.current;
        }

        private float EyeY => _planes.FloorHeight + _config.EyeHeight;

        public void Start()
        {
            _input.Dragged += OnDragged;
            _input.Tapped += OnTapped;

            if (_gyro != null)
                InputSystem.EnableDevice(_gyro);

            var start = _camera.position;
            _target = new Vector3(start.x, EyeY, start.z);
            _camera.position = _target;
            _yaw = _camera.eulerAngles.y;
        }

        public void Dispose()
        {
            _input.Dragged -= OnDragged;
            _input.Tapped -= OnTapped;

            if (_gyro != null && _gyro.added)
                InputSystem.DisableDevice(_gyro);
        }

        public void Tick()
        {
            _camera.SetPositionAndRotation(
                Vector3.MoveTowards(_camera.position, _target, _config.WalkSpeed * Time.deltaTime),
                CurrentRotation());
        }

        private Quaternion CurrentRotation()
        {
            if (!TryReadGyro(out var device))
                return Quaternion.Euler(_pitch, _yaw, 0f);

            // The first reading is anchored to the camera's starting heading, so the player always faces
            // into the room no matter which way they hold the phone.
            if (!_hasGyroReference)
            {
                _gyroYawOffset = -device.eulerAngles.y;
                _hasGyroReference = true;
            }

            return Quaternion.Euler(0f, _yaw + _gyroYawOffset, 0f) * device;
        }

        private bool TryReadGyro(out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            if (_gyro == null || !_gyro.enabled)
                return false;

            var attitude = _gyro.attitude.ReadValue();
            // The sensor reports a zero quaternion until its first sample arrives.
            if (attitude.x == 0f && attitude.y == 0f && attitude.z == 0f && attitude.w == 0f)
                return false;

            rotation = DeviceToWorld * new Quaternion(attitude.x, attitude.y, -attitude.z, -attitude.w);
            return true;
        }

        private void OnDragged(Vector2 delta)
        {
            // Sensitivity is in degrees per full-screen swipe, so it feels the same on any resolution.
            var degreesPerPixel = _config.LookSensitivity / Screen.height;
            _yaw += delta.x * degreesPerPixel;

            // With a gyro the phone itself sets the pitch; the swipe only turns around.
            if (!_hasGyroReference)
                _pitch = Mathf.Clamp(_pitch - delta.y * degreesPerPixel, -_config.PitchLimit, _config.PitchLimit);
        }

        private void OnTapped(Vector3 point)
        {
            var bounds = _planes.RoomBounds;
            var destination = new Vector3(
                Mathf.Clamp(point.x, bounds.min.x, bounds.max.x),
                EyeY,
                Mathf.Clamp(point.z, bounds.min.z, bounds.max.z));
            _target = StopBeforeObstacles(_camera.position, destination);
        }

        private Vector3 StopBeforeObstacles(Vector3 from, Vector3 to)
        {
            var path = new Vector3(to.x - from.x, 0f, to.z - from.z);
            var distance = path.magnitude;
            if (distance < MinMoveDistance)
                return new Vector3(from.x, EyeY, from.z);

            var direction = path / distance;
            // The body capsule starts above the step height, so the floor and rugs never block it — only furniture and walls.
            var feet = new Vector3(from.x, _planes.FloorHeight + _config.StepHeight + _config.BodyRadius, from.z);
            var head = new Vector3(from.x, EyeY, from.z);
            if (Physics.CapsuleCast(feet, head, _config.BodyRadius, direction, out var hit, distance,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                distance = hit.distance;

            var end = from + direction * distance;
            return new Vector3(end.x, EyeY, end.z);
        }
    }
}
