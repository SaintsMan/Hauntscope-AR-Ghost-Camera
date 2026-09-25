using System;
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
        private bool _planesVisible = true;

        public ArPlaneProvider(ARPlaneManager planeManager)
        {
            _planeManager = planeManager;
            _planeManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }

        public float HorizontalArea
        {
            get
            {
                var area = 0f;
                foreach (var plane in _planeManager.trackables)
                {
                    if (plane.alignment != PlaneAlignment.HorizontalUp || plane.subsumedBy != null)
                        continue;

                    area += CalculatePolygonArea(plane.boundary);
                }

                return area;
            }
        }

        public void SetPlanesVisible(bool visible)
        {
            _planesVisible = visible;
            foreach (var plane in _planeManager.trackables)
                plane.gameObject.SetActive(visible);
        }

        public void Dispose()
        {
            _planeManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
        {
            foreach (var plane in changes.added)
                plane.gameObject.SetActive(_planesVisible);
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
