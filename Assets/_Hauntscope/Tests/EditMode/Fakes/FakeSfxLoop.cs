using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeSfxLoop : ISfxLoop
    {
        public FakeSfxLoop(float volume)
        {
            Volume = volume;
            Pitch = 1f;
        }

        public float Volume { get; private set; }

        public float Pitch { get; private set; }

        public Vector3 Position { get; private set; }

        public bool IsStopped { get; private set; }

        public bool IsPaused { get; private set; }

        public void SetVolume(float volume)
        {
            Volume = volume;
        }

        public void SetPitch(float pitch)
        {
            Pitch = pitch;
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
        }

        public void Stop()
        {
            IsStopped = true;
        }
    }
}
