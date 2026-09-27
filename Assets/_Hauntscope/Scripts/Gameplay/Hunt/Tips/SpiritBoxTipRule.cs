using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first hunt with the Spirit Box issued: say what it is for; switching it on answers the tip.
    public sealed class SpiritBoxTipRule : IFieldTipRule
    {
        private readonly SpiritBox _spiritBox;

        public SpiritBoxTipRule(SpiritBox spiritBox)
        {
            _spiritBox = spiritBox;
        }

        public FieldTipId Id => FieldTipId.SpiritBox;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            return context.Ghost != null && _spiritBox.IsUnlocked;
        }

        public bool IsResolved(FieldTipContext context)
        {
            return _spiritBox.IsActive.Value;
        }

        public void Reset()
        {
        }
    }
}
