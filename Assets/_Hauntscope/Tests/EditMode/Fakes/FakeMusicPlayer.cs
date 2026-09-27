using System.Collections.Generic;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeMusicPlayer : IMusicPlayer
    {
        public IReadOnlyList<AudioClip> Layers { get; private set; }

        public float Pitch { get; private set; }

        public int PlayCount { get; private set; }

        public Dictionary<int, float> Volumes { get; } = new Dictionary<int, float>();

        public List<AudioClip> Stingers { get; } = new List<AudioClip>();

        public bool IsStopped { get; private set; }

        public void Play(IReadOnlyList<AudioClip> layers, float pitch)
        {
            Layers = layers;
            Pitch = pitch;
            PlayCount++;
        }

        public void SetLayerVolume(int layer, float volume)
        {
            Volumes[layer] = volume;
        }

        public void PlayStinger(AudioClip clip, float volume)
        {
            Stingers.Add(clip);
        }

        public void Stop()
        {
            IsStopped = true;
        }
    }
}
