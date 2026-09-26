namespace Hauntscope.Gameplay.Shift
{
    // A spare cassette: extra frames in every hunt left in the shift.
    public sealed class FilmPerk : IShiftPerk
    {
        private readonly int _frames;

        public FilmPerk(int frames)
        {
            _frames = frames;
        }

        public void OnChosen(ShiftPerkTarget target)
        {
        }

        public void OnRoundStarted(ShiftPerkTarget target)
        {
            target.AddFilm(_frames);
        }
    }
}
