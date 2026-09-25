using UnityEngine;

namespace Hauntscope.Core.Services
{
    public interface ISfxLoop
    {
        void SetVolume(float volume);

        void SetPitch(float pitch);

        void SetPosition(Vector3 position);

        void SetPaused(bool paused);

        void Stop();
    }
}
