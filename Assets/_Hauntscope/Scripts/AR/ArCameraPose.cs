using Hauntscope.Gameplay.Environment;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Hauntscope.AR
{
    public sealed class ArCameraPose : ICameraPose
    {
        private readonly Camera _camera;
        private readonly Transform _transform;

        public ArCameraPose(XROrigin origin)
        {
            _camera = origin.Camera;
            _transform = _camera.transform;
        }

        public Vector3 Position => _transform.position;

        public Vector3 Forward => _transform.forward;

        public Vector3 WorldToViewport(Vector3 worldPosition)
        {
            return _camera.WorldToViewportPoint(worldPosition);
        }
    }
}
