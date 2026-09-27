using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.33.6: a caught ghost posed in the room for a photo to fool friends with; no hunt, no battery.
    [Serializable]
    public sealed class PrankPhotoConfig
    {
        [SerializeField, Min(0)] private int _unlockCaptures = 1;
        [SerializeField, Min(0.1f)] private float _minDistance = 2f;
        [SerializeField, Min(0.1f)] private float _maxDistance = 4f;
        [SerializeField, Min(0.1f)] private float _aimDistance = 2.8f;
        [SerializeField, Min(0.05f)] private float _minScale = 0.5f;
        [SerializeField, Min(0.05f)] private float _maxScale = 1.6f;
        [SerializeField, Min(0f)] private float _dragGain = 2f;

        public PrankPhotoConfig()
        {
        }

        public PrankPhotoConfig(int unlockCaptures, float minDistance, float maxDistance, float aimDistance, float minScale, float maxScale,
            float dragGain)
        {
            _unlockCaptures = unlockCaptures;
            _minDistance = minDistance;
            _maxDistance = maxDistance;
            _aimDistance = aimDistance;
            _minScale = minScale;
            _maxScale = maxScale;
            _dragGain = dragGain;
        }

        public int UnlockCaptures => _unlockCaptures;

        // How near and far from the camera the ghost can stand, on the floor.
        public float MinDistance => _minDistance;

        public float MaxDistance => _maxDistance;

        // Where it stands while the camera looks above the floor's horizon.
        public float AimDistance => _aimDistance;

        public float MinScale => _minScale;

        public float MaxScale => _maxScale;

        // A drag across the whole screen height moves the ghost this many times its distance from the camera.
        public float DragGain => _dragGain;
    }
}
