using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // One thing the voice in the static said: which way, how far, and whether the ghost is hiding.
    public readonly struct SpiritBoxAnswer
    {
        public SpiritBoxAnswer(SpiritDirection direction, SpiritRange range, bool isHiding, Vector3 voicePosition)
        {
            Direction = direction;
            Range = range;
            IsHiding = isHiding;
            VoicePosition = voicePosition;
        }

        public SpiritDirection Direction { get; }

        public SpiritRange Range { get; }

        public bool IsHiding { get; }

        // Where the voice is played from: beside the camera, on the ghost's side.
        public Vector3 VoicePosition { get; }
    }
}
