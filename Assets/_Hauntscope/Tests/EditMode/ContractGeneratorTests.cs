using Hauntscope.Gameplay.Contracts;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ContractGeneratorTests
    {
        private ContractFixture _fixture;

        [TearDown]
        public void TearDown()
        {
            _fixture.Dispose();
        }

        [Test]
        public void Generate_Tier_PicksAContractOfThatTier()
        {
            _fixture = new ContractFixture();

            var slot = _fixture.Generator.Generate(ContractTier.Medium, new ContractSlot[0]);

            Assert.AreEqual(ContractTier.Medium, slot.Data.Tier);
            Assert.AreEqual(0, slot.Progress);
        }

        [Test]
        public void Generate_OneAlreadyOnTheBoard_PicksTheOther()
        {
            _fixture = new ContractFixture();
            _fixture.Random.DefaultValue = 0f;
            var board = new[] { _fixture.Generator.Generate(ContractTier.Easy, new ContractSlot[0]) };

            var slot = _fixture.Generator.Generate(ContractTier.Easy, board);

            Assert.AreSame(_fixture.CaptureAny, board[0].Data);
            Assert.AreSame(_fixture.Pickups, slot.Data);
        }

        [Test]
        public void Generate_ShiftLocked_OffersOnlyWhatThePlayerCanDo()
        {
            _fixture = new ContractFixture();
            _fixture.Random.DefaultValue = 0.99f;

            var slot = _fixture.Generator.Generate(ContractTier.Hard, new ContractSlot[0]);

            Assert.AreSame(_fixture.BatteryLeft, slot.Data);
        }

        [Test]
        public void Generate_NothingLeft_ReturnsNull()
        {
            _fixture = new ContractFixture();
            var board = new[] { _fixture.Generator.Generate(ContractTier.Hard, new ContractSlot[0]) };

            Assert.IsNull(_fixture.Generator.Generate(ContractTier.Hard, board));
        }

        [Test]
        public void Generate_Tier_PaysTheTierReward()
        {
            _fixture = new ContractFixture();

            var slot = _fixture.Generator.Generate(ContractTier.Medium, new ContractSlot[0]);

            Assert.AreEqual(ContractFixture.MediumReward, slot.Reward.Ectoplasm);
            Assert.IsNull(slot.Reward.Gear);
        }

        [Test]
        public void Generate_GearChanceHit_PaysABoosterInstead()
        {
            _fixture = new ContractFixture(hardGearChance: 0.5f);
            _fixture.Random.Enqueue(0f, 0.2f, 0.9f);

            var slot = _fixture.Generator.Generate(ContractTier.Hard, new ContractSlot[0]);

            Assert.AreEqual(0, slot.Reward.Ectoplasm);
            Assert.AreSame(_fixture.Store.Salt, slot.Reward.Gear);
            Assert.AreEqual(1, slot.Reward.GearCount);
        }
    }
}
