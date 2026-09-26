namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first time a ghost is left stunned by its own ability.
    public sealed class StaggerTipRule : IFieldTipRule
    {
        public FieldTipId Id => FieldTipId.Stagger;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            var ghost = context.Ghost;
            return ghost != null && ghost.IsStaggered && !context.IsFlushStagger;
        }

        public bool IsResolved(FieldTipContext context)
        {
            return context.Ghost == null || !context.Ghost.IsStaggered;
        }

        public void Reset()
        {
        }
    }
}
