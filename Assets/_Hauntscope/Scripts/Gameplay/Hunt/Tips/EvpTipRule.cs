using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Hunt.Tips
{
    // The first hunt with the EVP recorder issued: say what it is for; the first take answers the tip.
    public sealed class EvpTipRule : IFieldTipRule
    {
        private readonly EvpRecorder _recorder;

        public EvpTipRule(EvpRecorder recorder)
        {
            _recorder = recorder;
        }

        public FieldTipId Id => FieldTipId.Evp;

        public bool IsDue(FieldTipContext context, float deltaTime)
        {
            return context.Ghost != null && _recorder.IsUnlocked;
        }

        public bool IsResolved(FieldTipContext context)
        {
            return _recorder.Phase.Value != EvpPhase.Ready;
        }

        public void Reset()
        {
        }
    }
}
