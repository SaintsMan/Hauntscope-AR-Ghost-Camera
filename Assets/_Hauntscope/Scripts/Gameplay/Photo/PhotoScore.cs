namespace Hauntscope.Gameplay.Photo
{
    // One star per quality, so the HUD can show the player exactly what earned (or missed) each star.
    public readonly struct PhotoScore
    {
        public PhotoScore(bool isCentered, bool isClose, bool isMoment)
        {
            IsCentered = isCentered;
            IsClose = isClose;
            IsMoment = isMoment;
        }

        public bool IsCentered { get; }

        public bool IsClose { get; }

        public bool IsMoment { get; }

        public int Stars => (IsCentered ? 1 : 0) + (IsClose ? 1 : 0) + (IsMoment ? 1 : 0);
    }
}
