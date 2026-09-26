using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Shift
{
    // What a perk may change: the hunt's modifiers, the battery, and the film loaded for the round.
    public sealed class ShiftPerkTarget
    {
        public ShiftPerkTarget(HuntModifiers modifiers, Battery battery)
        {
            Modifiers = modifiers;
            Battery = battery;
        }

        public HuntModifiers Modifiers { get; }

        public Battery Battery { get; }

        // Frames on top of the camera's usual film for the round being started.
        public int ExtraFilm { get; private set; }

        public void BeginRound()
        {
            ExtraFilm = 0;
        }

        public void AddFilm(int frames)
        {
            ExtraFilm += frames;
        }
    }
}
