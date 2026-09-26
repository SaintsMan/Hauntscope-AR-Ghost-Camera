namespace Hauntscope.Gameplay.Hunt.Tips
{
    // From the second hunt on, the first time the shutter can fire.
    public sealed class PhotoTipRule : IFieldTipRule
    {
        public FieldTipId Id => FieldTipId.Photo;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            return context.Progress.TotalSessions >= context.Config.PhotoFromHunt && context.Camera.CanShoot.Value;
        }

        public bool IsResolved(FieldTipContext context)
        {
            return !context.Camera.CanShoot.Value;
        }

        public void Reset()
        {
        }
    }
}
