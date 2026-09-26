namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The beam has held a ghost from far off for a while: stepping in would charge it faster.
    public sealed class CloseInTipRule : IFieldTipRule
    {
        private float _held;

        public FieldTipId Id => FieldTipId.CloseIn;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            if (!IsHeldFromFar(context))
            {
                _held = 0f;
                return false;
            }

            _held += deltaTime;
            return _held >= context.Config.CloseInHold;
        }

        public bool IsResolved(FieldTipContext context)
        {
            return !IsHeldFromFar(context);
        }

        public void Reset()
        {
            _held = 0f;
        }

        private static bool IsHeldFromFar(FieldTipContext context)
        {
            return context.Beam.IsLocked.Value && context.Beam.GhostDistance > context.Config.CloseInDistance;
        }
    }
}
