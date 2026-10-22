using System;

namespace Hauntscope.Gameplay.Analytics
{
    // The hunt_end a hunt would report if the game never came back from the background.
    [Serializable]
    public sealed class UnfinishedHuntDto
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public bool IsPending;
        public string Outcome = string.Empty;
        public string Ghost = string.Empty;
        public int DurationSeconds;
        public string Environment = string.Empty;
        public int HuntNumber;
        public int MaxProgress;
        public int DistanceOnLostDm = -1;
    }
}
