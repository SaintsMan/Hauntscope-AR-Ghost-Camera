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
        private const float TwoPi = Mathf.PI * 2f;
        private const float SwaySeedYaw = 17.3f;
        private const float SwaySeedPitch = 41.7f;

        // Android reports attitude in a right-handed, Z-up frame; this maps it to Unity's left-handed, Y-up frame
        // for a phone held in portrait.
        private static readonly Quaternion DeviceToWorld = Quaternion.Euler(90f, 0f, 0f);

        private readonly Transform _camera;
        private readonly VirtualPointerInput _input;
        private readonly VirtualJoystick _joystick;
        private readonly IPlaneProvider _planes;
        private readonly VirtualConfig _config;
        private readonly AttitudeSensor _gyro;

        private Vector3 _position;
        private Vector3 _target;
        private float _yaw;
        private float _pitch;
        private float _gyroYawOffset;
        private bool _hasGyroReference;
        private float _stepPhase;
        private float _bobWeight;

        public VirtualCameraController(
            Camera camera,
            VirtualPointerInput input,
            VirtualJoystick joystick,
            IPlaneProvider planes,
            VirtualConfig config)
        {
            _camera = camera.transform;
            _input = input;
            _joystick = joystick;
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
            _position = new Vector3(start.x, EyeY, start.z);
            _target = _position;
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
            var deltaTime = Time.deltaTime;
            var rotation = CurrentRotation();
            var previous = _position;

            var stick = _joystick.Value;
            if (stick.sqrMagnitude > _config.JoystickDeadZone * _config.JoystickDeadZone)
            {
                Walk(rotation, stick, deltaTime);
                // The stick takes over from a pending tap, otherwise releasing it would resume an old walk.
                _target = _position;
            }
            else
            {
                _position = Vector3.MoveTowards(_position, _target, _config.WalkSpeed * deltaTime);
            }

            var speed = deltaTime > 0f ? (_position - previous).magnitude / deltaTime : 0f;
            _camera.SetPositionAndRotation(_position + HeadBob(rotation, speed, deltaTime), rotation * HandheldSway());
        }

        private void Walk(Quaternion rotation, Vector2 stick, float deltaTime)
        {
            var forward = rotation * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();
            var right = new Vector3(forward.z, 0f, -forward.x);
            var step = (forward * stick.y + right * stick.x) * (_config.WalkSpeed * deltaTime);

            // Blocked moves slide along the obstacle instead of stopping dead, so walls guide the player.
            if (TryCast(_position, step, out var hit))
            {
                var slide = Vector3.ProjectOnPlane(step, hit.normal);
                slide.y = 0f;
                step = TryCast(_position, slide, out _) ? Vector3.zero : slide;
            }

            _position = ClampToRoom(_position + step);
        }

        private Vector3 HeadBob(Quaternion rotation, float speed, float deltaTime)
        {
            // Footsteps only while actually moving; the weight eases in and out so stopping doesn't snap.
            var moving = Mathf.Clamp01(speed / _config.WalkSpeed);
            _bobWeight = Mathf.MoveTowards(_bobWeight, moving, deltaTime * _config.BobFrequency * 2f);
            _stepPhase += deltaTime * _config.BobFrequency * TwoPi * moving;

            var vertical = Mathf.Abs(Mathf.Sin(_stepPhase)) * _config.BobAmplitude;
            var lateral = Mathf.Sin(_stepPhase * 0.5f) * _config.BobAmplitude * 0.6f;
            return (Vector3.up * vertical + rotation * Vector3.right * lateral) * _bobWeight;
        }

        private Quaternion HandheldSway()
        {
            // A real hand already moves the phone when the gyro drives the view; faking it on top would fight it.
            if (_hasGyroReference || _config.SwayAngle <= 0f)
                return Quaternion.identity;

            var time = Time.time * _config.SwayFrequency;
            var yaw = (Mathf.PerlinNoise(time, SwaySeedYaw) - 0.5f) * 2f * _config.SwayAngle;
            var pitch = (Mathf.PerlinNoise(SwaySeedPitch, time) - 0.5f) * 2f * _config.SwayAngle;
            return Quaternion.Euler(pitch, yaw, 0f);
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
            var destination = ClampToRoom(new Vector3(point.x, EyeY, point.z));
            _target = StopBeforeObstacles(_position, destination);
        }

        private Vector3 ClampToRoom(Vector3 position)
        {
            var bounds = _planes.RoomBounds;
            return new Vector3(
                Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
                EyeY,
                Mathf.Clamp(position.z, bounds.min.z, bounds.max.z));
        }

        private Vector3 StopBeforeObstacles(Vector3 from, Vector3 to)
        {
            var path = new Vector3(to.x - from.x, 0f, to.z - from.z);
            if (path.magnitude < MinMoveDistance)
                return new Vector3(from.x, EyeY, from.z);

            if (TryCast(from, path, out var hit))
                path = path.normalized * hit.distance;

            var end = from + path;
            return new Vector3(end.x, EyeY, end.z);
        }

        private bool TryCast(Vector3 from, Vector3 move, out RaycastHit hit)
        {
            var distance = move.magnitude;
            if (distance < Mathf.Epsilon)
            {
                hit = default;
                return false;
            }

            // The body capsule starts above the step height, so the floor and rugs never block it — only furniture and walls.
            var feet = new Vector3(from.x, _planes.FloorHeight + _config.StepHeight + _config.BodyRadius, from.z);
            var head = new Vector3(from.x, EyeY, from.z);
            return Physics.CapsuleCast(feet, head, _config.BodyRadius, move / distance, out hit, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }
    }
}
