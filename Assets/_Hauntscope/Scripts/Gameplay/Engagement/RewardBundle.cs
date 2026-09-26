using Hauntscope.Gameplay.Store;

namespace Hauntscope.Gameplay.Engagement
{
    // A reward handed out in one go: ectoplasm and, optionally, a piece of gear.
    public sealed class RewardBundle
    {
        public RewardBundle(int ectoplasm, GearData gear = null, int gearCount = 0)
        {
            Ectoplasm = ectoplasm;
            Gear = gear;
            GearCount = gear != null ? gearCount : 0;
        }

        public int Ectoplasm { get; }

        public GearData Gear { get; }

        public int GearCount { get; }
    }
}
