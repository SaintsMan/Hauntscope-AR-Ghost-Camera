namespace Hauntscope.Gameplay.Feedback
{
    // What the hunt is doing right now, as far as the music cares.
    public readonly struct HuntMusicMoment
    {
        public HuntMusicMoment(bool isHunting, float closeness, bool isRevealed, bool isCapturing, bool isSurging, bool hasResult,
            bool isPaused, bool isRecording, bool isPrank)
        {
            IsHunting = isHunting;
            Closeness = closeness;
            IsRevealed = isRevealed;
            IsCapturing = isCapturing;
            IsSurging = isSurging;
            HasResult = hasResult;
            IsPaused = isPaused;
            IsRecording = isRecording;
            IsPrank = isPrank;
        }

        // A ghost is in the room and being hunted (the room is scanned, no result yet).
        public bool IsHunting { get; }

        // The EMF level as a share of its top: 0 far, 1 right there.
        public float Closeness { get; }

        public bool IsRevealed { get; }

        public bool IsCapturing { get; }

        public bool IsSurging { get; }

        public bool HasResult { get; }

        public bool IsPaused { get; }

        // The EVP recorder's tape is running and must be heard.
        public bool IsRecording { get; }

        public bool IsPrank { get; }
    }
}
