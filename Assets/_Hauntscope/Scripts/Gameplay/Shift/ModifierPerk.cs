using Hauntscope.Gameplay.Store;

namespace Hauntscope.Gameplay.Shift
{
    // Colder lens, stronger antenna, heavier hand, wider beam, salt: a lasting change to how the hunt plays.
    public sealed class ModifierPerk : IShiftPerk
    {
        private readonly HuntModifierSet _modifiers;

        public ModifierPerk(HuntModifierSet modifiers)
        {
            _modifiers = modifiers;
        }

        public void OnChosen(ShiftPerkTarget target)
        {
        }

        public void OnRoundStarted(ShiftPerkTarget target)
        {
            target.Modifiers.Apply(_modifiers);
        }
    }
}
