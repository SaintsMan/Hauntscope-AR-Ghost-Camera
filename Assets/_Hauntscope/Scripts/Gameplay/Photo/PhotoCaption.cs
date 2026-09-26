using System;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Photo
{
    // What the camcorder frame burns into the picture: case file, stars and the moment it was taken.
    public sealed class PhotoCaption
    {
        public PhotoCaption(GhostData ghost, int stars, DateTime takenLocal)
        {
            Ghost = ghost;
            Stars = stars;
            TakenLocal = takenLocal;
        }

        public GhostData Ghost { get; }

        public int Stars { get; }

        public DateTime TakenLocal { get; }
    }
}
