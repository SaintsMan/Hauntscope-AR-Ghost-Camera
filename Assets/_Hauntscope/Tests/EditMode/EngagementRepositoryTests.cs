using System.Collections.Generic;
using Hauntscope.Gameplay.Contracts;
using Hauntscope.Gameplay.Engagement;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class EngagementRepositoryTests
    {
        private ContractFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ContractFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture.Dispose();
        }

        [Test]
        public void Load_NothingSaved_StartsEmpty()
        {
            var progress = _fixture.Repository.Load();

            Assert.AreEqual(0, progress.ContractDay);
            Assert.IsEmpty(progress.Contracts);
        }

        [Test]
        public void Load_AfterSave_RestoresTheBoard()
        {
            var slots = new List<ContractSlot>
            {
                new ContractSlot(_fixture.CaptureAny, string.Empty, new RewardBundle(15), 1),
                new ContractSlot(_fixture.BatteryLeft, "wraith", new RewardBundle(0, _fixture.Store.Salt, 1), 1, true)
            };
            var progress = new EngagementProgress(ContractFixture.Today, slots, 1, 0);

            _fixture.Repository.Save(progress);
            var loaded = _fixture.Repository.Load();

            Assert.AreEqual(ContractFixture.Today, loaded.ContractDay);
            Assert.AreEqual(1, loaded.FreeReplacesUsed);
            Assert.AreSame(_fixture.CaptureAny, loaded.Contracts[0].Data);
            Assert.AreEqual(1, loaded.Contracts[0].Progress);
            Assert.AreEqual(15, loaded.Contracts[0].Reward.Ectoplasm);
            Assert.AreEqual("wraith", loaded.Contracts[1].Subject);
            Assert.AreSame(_fixture.Store.Salt, loaded.Contracts[1].Reward.Gear);
            Assert.IsTrue(loaded.Contracts[1].IsClaimed);
        }

        [Test]
        public void Load_ContractNoLongerInTheGame_DropsIt()
        {
            var removed = ContractFixture.Contract("retired", ContractTier.Easy, 1, new CaptureCountGoal());
            var slots = new List<ContractSlot>
            {
                new ContractSlot(removed, string.Empty, new RewardBundle(15)),
                new ContractSlot(_fixture.FlushOut, string.Empty, new RewardBundle(30))
            };
            _fixture.Repository.Save(new EngagementProgress(ContractFixture.Today, slots, 0, 0));

            var loaded = _fixture.Repository.Load();

            Assert.AreEqual(1, loaded.Contracts.Count);
            Assert.AreSame(_fixture.FlushOut, loaded.Contracts[0].Data);
            UnityEngine.Object.DestroyImmediate(removed);
        }
    }
}
