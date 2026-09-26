namespace Hauntscope.Gameplay.Shift
{
    // One perk chosen at a shift break. Some act once, the moment they are chosen; others at the start of every later
    // round of the shift. Perks stack: two of the same apply twice.
    public interface IShiftPerk
    {
        void OnChosen(ShiftPerkTarget target);

        void OnRoundStarted(ShiftPerkTarget target);
    }
}
