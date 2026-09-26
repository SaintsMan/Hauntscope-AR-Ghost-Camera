namespace Hauntscope.Gameplay.Shift
{
    public enum ShiftPhase
    {
        // A single hunt, or no shift started yet.
        Off,
        Hunting,
        // A catch before the last round: its result card is up and leads on to the break.
        BetweenRounds,
        // Choosing a perk.
        Break,
        Ended
    }
}
