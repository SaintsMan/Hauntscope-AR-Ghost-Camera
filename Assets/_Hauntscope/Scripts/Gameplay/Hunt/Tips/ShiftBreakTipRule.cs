using Hauntscope.Gameplay.Shift;
namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first break of a night shift; it stays until a perk is chosen.
    public sealed class ShiftBreakTipRule : IFieldTipRule
    {
        public FieldTipId Id => FieldTipId.ShiftBreak;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            return context.Shift.Phase.Value == ShiftPhase.Break;
        }

        public bool IsResolved(FieldTipContext context)
        {
            return context.Shift.Phase.Value != ShiftPhase.Break;
        }

        public void Reset()
        {
        }
    }
}
