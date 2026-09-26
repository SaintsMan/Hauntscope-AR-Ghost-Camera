using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Hauntscope.AR
{
    public sealed class ArPlaneProvider : IPlaneProvider, IDisposable
    {
        private readonly ARPlaneManager _planeManager;
        private readonly RoomConfig _config;
        private bool _planesVisible = true;

        public ArPlaneProvider(ARPlaneManager planeManager, RoomConfig config)
        {
            _planeManager = planeManager;
            _config = config;
            _planeManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }

        public float HorizontalArea { get; private set; }

        public Bounds RoomBounds { get; private set; }

        public float FloorHeight { get; private set; }

        public void SetPlanesVisible(bool visible)
        {
            _planesVisible = visible;
            foreach (var plane in _planeManager.trackables)
                plane.gameObject.SetActive(visible);
        }

        public bool IsFloorPoint(Vector3 point)
        {
            foreach (var plane in _planeManager.trackables)
            {
                if (plane.alignment != PlaneAlignment.HorizontalUp || plane.subsumedBy != null)
                    continue;
                if (Mathf.Abs(plane.transform.position.y - FloorHeight) > _config.FloorTolerance)
                    continue;

                var local = plane.transform.InverseTransformPoint(point);
                if (IsInside(plane.boundary, new Vector2(local.x, local.z)))
                    return true;
            }

            return false;
        }

        public void Dispose()
        {
            _planeManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
        {
            foreach (var plane in changes.added)
                plane.gameObject.SetActive(_planesVisible);

            Recalculate();
        }

        private void Recalculate()
        {
            var area = 0f;
            var floor = float.PositiveInfinity;
            var hasBounds = false;
            var bounds = new Bounds();

            foreach (var plane in _planeManager.trackables)
            {
                if (plane.alignment != PlaneAlignment.HorizontalUp || plane.subsumedBy != null)
                    continue;

                area += CalculatePolygonArea(plane.boundary);
                floor = Mathf.Min(floor, plane.transform.position.y);

                foreach (var point in plane.boundary)
                {
                    var world = plane.transform.TransformPoint(new Vector3(point.x, 0f, point.y));
                    if (hasBounds)
                    {
                        bounds.Encapsulate(world);
                    }
                    else
                    {
                        bounds = new Bounds(world, Vector3.zero);
                        hasBounds = true;
                    }
                }
            }

            var padding = _config.RoomBoundsPadding * 2f;
            bounds.Expand(new Vector3(padding, 0f, padding));

            HorizontalArea = area;
            RoomBounds = bounds;
            FloorHeight = hasBounds ? floor : 0f;
        }

        private static bool IsInside(NativeArray<Vector2> polygon, Vector2 point)
        {
            var inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if ((a.y > point.y) != (b.y > point.y) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }

            return inside;
        }

        private static float CalculatePolygonArea(NativeArray<Vector2> boundary)
        {
            var doubledArea = 0f;
            for (int i = 0, j = boundary.Length - 1; i < boundary.Length; j = i++)
                doubledArea += boundary[j].x * boundary[i].y - boundary[i].x * boundary[j].y;

            return Mathf.Abs(doubledArea) * 0.5f;
        }
    }
}
