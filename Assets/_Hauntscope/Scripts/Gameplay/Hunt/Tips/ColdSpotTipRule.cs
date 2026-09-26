namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first hiding spot that is in frame or loud on the EMF.
    public sealed class ColdSpotTipRule : IFieldTipRule
    {
        public FieldTipId Id => FieldTipId.ColdSpot;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            var ghost = context.Ghost;
            return ghost != null && ghost.IsHiding
                && (context.Radar.Level.Value >= context.Config.ColdSpotEmfLevel || context.IsInFrame(ghost.HideSpot));
        }

        public bool IsResolved(FieldTipContext context)
        {
            return context.Ghost == null || !context.Ghost.IsHiding;
        }

        public void Reset()
        {
        }
    }
}
