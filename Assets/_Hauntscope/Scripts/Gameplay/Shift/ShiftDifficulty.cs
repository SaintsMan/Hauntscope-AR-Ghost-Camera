using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Store;

namespace Hauntscope.Gameplay.Shift
{
    // GDD 5.27: each round of a shift the ghost resists more and moves faster, the pool leans rarer and the catch pays
    // more. Tougher resistance is the same as a slower beam, so it goes in as a capture-rate modifier.
    public sealed class ShiftDifficulty
    {
        private readonly ShiftConfig _config;

        public ShiftDifficulty(ShiftConfig config)
        {
            _config = config;
        }

        public float ResistanceMultiplier(int round)
        {
            return 1f + _config.ResistanceStep * round;
        }

        public float SpeedMultiplier(int round)
        {
            return 1f + _config.SpeedStep * round;
        }

        public HuntModifierSet ModifiersFor(int round)
        {
            return new HuntModifierSet(captureRate: 1f / ResistanceMultiplier(round), ghostSpeed: SpeedMultiplier(round));
        }

        public RarityWeights WeightsFor(int round)
        {
            return _config.Weights(round);
        }

        public float RewardMultiplier(int round)
        {
            return _config.RewardMultiplier(round);
        }
    }
}
