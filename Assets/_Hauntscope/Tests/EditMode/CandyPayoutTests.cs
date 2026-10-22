using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Season;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class CandyPayoutTests
    {
        private HalloweenFixture _event;
        private HuntLoot _loot;
        private CandyPayout _payout;

        [SetUp]
        public void SetUp()
        {
            _event = new HalloweenFixture();
            _loot = new HuntLoot();
            _payout = new CandyPayout(_loot, _event.Collection);
            _payout.Start();
        }

        [Test]
        public void Dispose_PlayerQuitTheHunt_PaysTheCandyFound()
        {
            _loot.AddCandy(2);

            _payout.Dispose();

            Assert.AreEqual(2, _event.Collection.Candy.Value);
        }

        [Test]
        public void Dispose_AfterTheResultPaid_PaysNothingMore()
        {
            _loot.AddCandy(2);
            _payout.Pay();

            _payout.Dispose();

            Assert.AreEqual(2, _event.Collection.Candy.Value);
        }

        [Test]
        public void Pay_NextHuntAfterReset_PaysItsOwnCandy()
        {
            _loot.AddCandy(2);
            _payout.Pay();
            _loot.Reset();
            _loot.AddCandy(1);

            _payout.Pay();

            Assert.AreEqual(3, _event.Collection.Candy.Value);
        }

        [Test]
        public void Pay_CandyFoundInTheHunt_GoesToThePurse()
        {
            _loot.AddCandy(3);

            _payout.Pay();

            Assert.AreEqual(3, _event.Collection.Candy.Value);
        }

        [Test]
        public void Pay_NothingFound_LeavesThePurseAlone()
        {
            _payout.Pay();

            Assert.AreEqual(0, _event.Collection.Candy.Value);
        }

        [Test]
        public void Reset_NewHunt_ForgetsTheCandyOfTheLastOne()
        {
            _loot.AddCandy(3);

            _loot.Reset();

            Assert.AreEqual(0, _loot.Candy.Value);
        }
    }
}
