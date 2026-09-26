using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Hauntscope.AR
{
    // Hiding spots in a real room: at the foot of every wall or cupboard ARCore has found, otherwise the corners of
    // the room. With depth occlusion on, the real furniture then hides the ghost by itself.
    public sealed class ArHideSpotProvider : IHideSpotProvider, IDisposable
    {
        private readonly ARPlaneManager _planeManager;
        private readonly IPlaneProvider _planes;
        private readonly RoomCalibration _calibration;
        private readonly HideConfig _config;
        private readonly List<Vector3> _spots = new List<Vector3>();

        public ArHideSpotProvider(ARPlaneManager planeManager, IPlaneProvider planes, RoomCalibration calibration, HideConfig config)
        {
            _planeManager = planeManager;
            _planes = planes;
            _calibration = calibration;
            _config = config;
            _calibration.Progress.Changed += OnCalibrationChanged;
        }

        public IReadOnlyList<Vector3> Spots
        {
            get
            {
                Collect();
                return _spots;
            }
        }

        public void Dispose()
        {
            _calibration.Progress.Changed -= OnCalibrationChanged;
        }

        // Walls are only looked for once the floor is scanned: the scan stays about the floor, and hiding spots are
        // needed only later in the hunt anyway.
        private void OnCalibrationChanged(float progress)
        {
            if (_calibration.IsComplete)
                _planeManager.requestedDetectionMode |= PlaneDetectionMode.Vertical;
        }

        private void Collect()
        {
            _spots.Clear();
            var bounds = _planes.RoomBounds;
            var floor = _planes.FloorHeight;
            foreach (var plane in _planeManager.trackables)
            {
                if (plane.alignment != PlaneAlignment.Vertical || plane.subsumedBy != null)
                    continue;

                var center = plane.center;
                var inward = plane.normal;
                inward.y = 0f;
                if (inward.sqrMagnitude <= 0f)
                    continue;

                inward.Normalize();
                // The plane's normal may face either way; the spot belongs on the room side of the wall.
                var toRoom = bounds.center - center;
                if (inward.x * toRoom.x + inward.z * toRoom.z < 0f)
                    inward = -inward;

                _spots.Add(Inside(center + inward * _config.WallOffset, bounds, floor));
            }

            if (_spots.Count > 0)
                return;

            var inset = _config.CornerInset;
            _spots.Add(Inside(new Vector3(bounds.min.x + inset, floor, bounds.min.z + inset), bounds, floor));
            _spots.Add(Inside(new Vector3(bounds.min.x + inset, floor, bounds.max.z - inset), bounds, floor));
            _spots.Add(Inside(new Vector3(bounds.max.x - inset, floor, bounds.min.z + inset), bounds, floor));
            _spots.Add(Inside(new Vector3(bounds.max.x - inset, floor, bounds.max.z - inset), bounds, floor));
        }

        private static Vector3 Inside(Vector3 point, Bounds bounds, float floor)
        {
            return new Vector3(
                Mathf.Clamp(point.x, bounds.min.x, bounds.max.x),
                floor,
                Mathf.Clamp(point.z, bounds.min.z, bounds.max.z));
        }
    }
}
