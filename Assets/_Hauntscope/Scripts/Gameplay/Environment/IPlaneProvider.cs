namespace Hauntscope.Gameplay.Environment
{
    public interface IPlaneProvider
    {
        float HorizontalArea { get; }

        void SetPlanesVisible(bool visible);
    }
}
