using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePlaneProvider : IPlaneProvider
    {
        public float HorizontalArea { get; set; }

        public bool PlanesVisible { get; private set; }

        public void SetPlanesVisible(bool visible)
        {
            PlanesVisible = visible;
        }
    }
}
