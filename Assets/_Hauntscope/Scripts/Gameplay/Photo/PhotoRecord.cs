namespace Hauntscope.Gameplay.Photo
{
    public sealed class PhotoRecord
    {
        public PhotoRecord(string fileName, string ghostId, int stars, long takenUnixSeconds)
        {
            FileName = fileName;
            GhostId = ghostId;
            Stars = stars;
            TakenUnixSeconds = takenUnixSeconds;
        }

        public string FileName { get; }

        public string GhostId { get; }

        public int Stars { get; }

        public long TakenUnixSeconds { get; }
    }
}
