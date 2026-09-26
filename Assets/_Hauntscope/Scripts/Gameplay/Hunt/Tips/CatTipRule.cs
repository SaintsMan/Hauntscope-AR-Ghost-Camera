namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first hunt with the phantom cat.
    public sealed class CatTipRule : IFieldTipRule
    {
        public FieldTipId Id => FieldTipId.Cat;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            var data = context.Session.GhostData;
            return context.Ghost != null && data != null && data.Id == context.Config.CatGhostId;
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
