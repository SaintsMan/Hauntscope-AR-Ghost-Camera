using UnityEngine;

namespace Hauntscope.Gameplay.Environment
{
    public interface IPlaneProvider
    {
        float HorizontalArea { get; }

        Bounds RoomBounds { get; }

        float FloorHeight { get; }

        void SetPlanesVisible(bool visible);

        // True when the point lies on free floor: pickups must not end up inside furniture or on a table.
        bool IsFloorPoint(Vector3 point);
    }
}
