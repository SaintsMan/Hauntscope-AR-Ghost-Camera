using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.VirtualRoom
{
    public sealed class VirtualCameraPose : ICameraPose
    {
        private readonly Camera _camera;
        private readonly Transform _transform;

        public VirtualCameraPose(Camera camera)
        {
            _camera = camera;
            _transform = camera.transform;
        }

        public Vector3 Position => _transform.position;

        public Vector3 Forward => _transform.forward;

        public float Aspect => _camera.aspect;

        public Vector3 WorldToViewport(Vector3 worldPosition)
        {
            return _camera.WorldToViewportPoint(worldPosition);
        }
    }
}
