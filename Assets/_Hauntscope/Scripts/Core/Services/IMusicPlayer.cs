using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Core.Services
{
    // The soundtrack: a set of loops started together and kept sample-locked, each at its own level, and one-shot
    // stingers over them. Levels are targets the player eases towards, so callers can set them every frame.
    public interface IMusicPlayer
    {
        // A set already playing keeps playing (only its pitch follows); a new one crossfades in over the old.
        void Play(IReadOnlyList<AudioClip> layers, float pitch);

        void SetLayerVolume(int layer, float volume);

        void PlayStinger(AudioClip clip, float volume);

        void Stop();
    }
}
