namespace Hauntscope.Core.Services
{
    // An effect that keeps running (a cold spot) until its owner stops it; stopped, it fades out on its own.
    public interface IVfxLoop
    {
        void Stop();
    }
}
