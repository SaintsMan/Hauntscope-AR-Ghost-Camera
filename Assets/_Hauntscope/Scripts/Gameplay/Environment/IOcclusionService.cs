namespace Hauntscope.Gameplay.Environment
{
    public interface IOcclusionService
    {
        bool IsSupported { get; }

        void SetEnabled(bool enabled);
    }
}
