namespace Hauntscope.Gameplay.Photo
{
    public sealed class PhotoShot
    {
        public PhotoShot(PhotoRecord record, PhotoScore score)
        {
            Record = record;
            Score = score;
        }

        public PhotoRecord Record { get; }

        public PhotoScore Score { get; }
    }
}
