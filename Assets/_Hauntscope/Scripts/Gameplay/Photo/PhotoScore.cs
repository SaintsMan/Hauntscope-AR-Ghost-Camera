namespace Hauntscope.Gameplay.Photo
{
    // One star per quality, so the HUD can show the player exactly what earned (or missed) each star.
    public readonly struct PhotoScore
    {
        public PhotoScore(bool isCentered, bool isFramed, bool isMoment)
        {
            IsCentered = isCentered;
            IsFramed = isFramed;
            IsMoment = isMoment;
        }

        public bool IsCentered { get; }

        // The whole ghost is in the picture and big enough to recognise.
        public bool IsFramed { get; }

        public bool IsMoment { get; }

        public int Stars => (IsCentered ? 1 : 0) + (IsFramed ? 1 : 0) + (IsMoment ? 1 : 0);
    }
}
