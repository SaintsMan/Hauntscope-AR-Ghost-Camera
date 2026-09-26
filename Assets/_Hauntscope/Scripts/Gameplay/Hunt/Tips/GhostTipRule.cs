namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first hunt with a ghost that plays by its own rules (the cat, the negative, the mara...): its tip, once.
    public sealed class GhostTipRule : IFieldTipRule
    {
        private readonly string _ghostId;

        public GhostTipRule(FieldTipId id, string ghostId)
        {
            Id = id;
            _ghostId = ghostId;
        }

        public FieldTipId Id { get; }

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            var data = context.Session.GhostData;
            return context.Ghost != null && data != null && data.Id == _ghostId;
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
