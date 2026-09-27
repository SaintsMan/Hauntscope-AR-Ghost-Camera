namespace Hauntscope.Gameplay.Tools
{
    // The ectoplasm decals on the floor, one per trail slot.
    public interface IUvTrailView
    {
        void SetMark(int slot, TrailMark mark, float visibility);
    }
}
