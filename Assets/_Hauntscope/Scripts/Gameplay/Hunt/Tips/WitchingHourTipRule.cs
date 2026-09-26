namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first hunt in the witching hour.
    public sealed class WitchingHourTipRule : IFieldTipRule
    {
        public FieldTipId Id => FieldTipId.WitchingHour;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            return context.Ghost != null && context.Session.IsWitchingHour;
        }

        public bool IsResolved(FieldTipContext context)
        {
            return false;
        }

        public void Reset()
        {
        }
    }
}
