using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Shift;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ShiftDifficultyTests
    {
        private ShiftFixture _fixture;
        private ShiftDifficulty _difficulty;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ShiftFixture();
            _difficulty = new ShiftDifficulty(_fixture.Config);
        }

        [TearDown]
        public void TearDown()
        {
            _fixture.Dispose();
        }

        [Test]
        public void ModifiersFor_FirstRound_ChangeNothing()
        {
            var modifiers = _difficulty.ModifiersFor(0);

            Assert.AreEqual(1f, modifiers.CaptureRate);
            Assert.AreEqual(1f, modifiers.GhostSpeed);
        }

        [Test]
        public void ModifiersFor_ThirdRound_ResistsAThirdMoreAndMovesAFifthFaster()
        {
            var modifiers = _difficulty.ModifiersFor(2);

            Assert.AreEqual(1f / 1.3f, modifiers.CaptureRate, 1e-4f);
            Assert.AreEqual(1.2f, modifiers.GhostSpeed, 1e-4f);
        }

        [Test]
        public void WeightsFor_ThirdRound_HasNoCommonGhosts()
        {
            var weights = _difficulty.WeightsFor(2);

            Assert.AreEqual(0f, weights.WeightOf(GhostRarity.Common));
            Assert.AreEqual(60f, weights.WeightOf(GhostRarity.Rare));
            Assert.AreEqual(40f, weights.WeightOf(GhostRarity.Legendary));
        }

        [Test]
        public void RewardMultiplier_PastTheLastRound_KeepsTheLastValue()
        {
            Assert.AreEqual(2f, _difficulty.RewardMultiplier(5));
        }
    }
}
