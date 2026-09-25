using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeSfxPlayer : ISfxPlayer
    {
        public int PlayCount { get; private set; }

        public float LastPitch { get; private set; }

        public int LoopCount { get; private set; }

        public FakeSfxLoop LastLoop { get; private set; }

        public void Play2D(AudioClip clip, float volume, float pitch)
        {
            PlayCount++;
            LastPitch = pitch;
        }

        public void Play3D(AudioClip clip, Vector3 position, float volume)
        {
            PlayCount++;
            LastPitch = 1f;
        }

        public ISfxLoop PlayLoop(AudioClip clip, float volume, bool spatial)
        {
            LoopCount++;
            LastLoop = new FakeSfxLoop(volume);
            return LastLoop;
        }
    }
}
