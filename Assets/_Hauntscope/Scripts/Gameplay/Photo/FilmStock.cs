using UnityEngine;

namespace Hauntscope.Gameplay.Photo
{
    // Spare frames of film kept between hunts (a promo code's gift). The camera loads them on top of the hunt's own
    // film and spends them only after its own frames are gone, so a gift is never used up by a hunt that did not need it.
    public sealed class FilmStock
    {
        public FilmStock(int frames = 0)
        {
            Frames = Mathf.Max(0, frames);
        }

        public int Frames { get; private set; }

        public void Add(int frames)
        {
            if (frames > 0)
                Frames += frames;
        }

        public bool TakeOne()
        {
            if (Frames <= 0)
                return false;

            Frames--;
            return true;
        }
    }
}
