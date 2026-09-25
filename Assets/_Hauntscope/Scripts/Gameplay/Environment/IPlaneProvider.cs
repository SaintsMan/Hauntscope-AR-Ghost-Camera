using UnityEngine;

namespace Hauntscope.Gameplay.Environment
{
    public interface IPlaneProvider
    {
        float HorizontalArea { get; }

        Bounds RoomBounds { get; }

        float FloorHeight { get; }

        void SetPlanesVisible(bool visible);
    }
}
