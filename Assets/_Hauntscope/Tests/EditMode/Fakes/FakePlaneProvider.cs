using System;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePlaneProvider : IPlaneProvider
    {
        public float HorizontalArea { get; set; }

        public Bounds RoomBounds { get; set; }

        public float FloorHeight { get; set; }

        public bool PlanesVisible { get; private set; }

        public Func<Vector3, bool> FloorTest { get; set; } = _ => true;

        public void SetPlanesVisible(bool visible)
        {
            PlanesVisible = visible;
        }

        public bool IsFloorPoint(Vector3 point)
        {
            return FloorTest(point);
        }
    }
}
