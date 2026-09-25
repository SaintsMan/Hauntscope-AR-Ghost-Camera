using UnityEngine;

namespace Hauntscope.Core.Services
{
    public interface ISfxPlayer
    {
        void Play2D(AudioClip clip, float volume, float pitch);

        void Play3D(AudioClip clip, Vector3 position, float volume);

        ISfxLoop PlayLoop(AudioClip clip, float volume, bool spatial);
    }
}
