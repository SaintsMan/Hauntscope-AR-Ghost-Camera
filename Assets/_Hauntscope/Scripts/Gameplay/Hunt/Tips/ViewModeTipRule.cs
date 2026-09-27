using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first hunt with a camera mode issued: say what it is for; switching to it answers the tip. One rule class
    // for every mode — each is registered with its own tip id.
    public sealed class ViewModeTipRule : IFieldTipRule
    {
        private readonly IViewMode _mode;

        public ViewModeTipRule(FieldTipId id, IViewMode mode)
        {
            Id = id;
            _mode = mode;
        }

        public FieldTipId Id { get; }

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            return context.Ghost != null && _mode.IsUnlocked;
        }

        public bool IsResolved(FieldTipContext context)
        {
            return _mode.IsActive.Value;
        }

        public void Reset()
        {
        }
    }
}
