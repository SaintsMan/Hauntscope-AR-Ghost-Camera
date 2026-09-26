using System;
using System.Threading;
using Hauntscope.Gameplay.Contracts;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ContractBoardTests
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
        public void Refresh_FirstTime_DealsOneContractPerTier()
        {
            _fixture.OpenFirstBoard();

            Assert.AreEqual(3, _fixture.Board.Slots.Count);
            Assert.AreSame(_fixture.CaptureAny, _fixture.Board.Slots[0].Data);
            Assert.AreSame(_fixture.FlushOut, _fixture.Board.Slots[1].Data);
            Assert.AreSame(_fixture.BatteryLeft, _fixture.Board.Slots[2].Data);
            Assert.AreEqual(ContractFixture.Today, _fixture.Board.Day);
        }

        [Test]
        public void Refresh_SameDay_KeepsTheBoard()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.Record(ContractFixture.Report(true));
            _fixture.Random.DefaultValue = 0.99f;

            _fixture.Board.Refresh();

            Assert.AreSame(_fixture.CaptureAny, _fixture.Board.Slots[0].Data);
            Assert.AreEqual(1, _fixture.Board.Slots[0].Progress);
        }

        [Test]
        public void Refresh_NextDay_DealsANewBoard()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.Record(ContractFixture.Report(true));
            _fixture.Clock.Today = _fixture.Clock.Today.AddDays(1);
            _fixture.Random.DefaultValue = 0.99f;

            _fixture.Board.Refresh();

            Assert.AreSame(_fixture.Pickups, _fixture.Board.Slots[0].Data);
            Assert.AreEqual(0, _fixture.Board.Slots[0].Progress);
        }

        [Test]
        public void Refresh_NextDayWithUnclaimedDoneContract_PaysItOnTheWay()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.Record(ContractFixture.Report(flushOuts: 1));
            _fixture.Clock.Today = _fixture.Clock.Today.AddDays(1);

            _fixture.Board.Refresh();

            Assert.AreEqual(ContractFixture.MediumReward, _fixture.Store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void Refresh_ClockSetBack_KeepsTheBoard()
        {
            _fixture.OpenFirstBoard();
            _fixture.Clock.Today = _fixture.Clock.Today.AddDays(-1);
            _fixture.Random.DefaultValue = 0.99f;

            _fixture.Board.Refresh();

            Assert.AreSame(_fixture.CaptureAny, _fixture.Board.Slots[0].Data);
            Assert.AreEqual(ContractFixture.Today, _fixture.Board.Day);
        }

        [Test]
        public void Record_Hunt_AdvancesMatchingContracts()
        {
            _fixture.OpenFirstBoard();

            _fixture.Board.Record(ContractFixture.Report(true, batteryLeft: 0.2f));

            Assert.AreEqual(1, _fixture.Board.Slots[0].Progress);
            Assert.AreEqual(0, _fixture.Board.Slots[2].Progress);
            Assert.IsEmpty(_fixture.Board.JustCompleted);
        }

        [Test]
        public void Record_CompletingHunt_ListsWhatItCompleted()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.Record(ContractFixture.Report(true));

            _fixture.Board.Record(ContractFixture.Report(true, flushOuts: 1));

            CollectionAssert.AreEqual(new[] { _fixture.Board.Slots[0], _fixture.Board.Slots[1] }, _fixture.Board.JustCompleted);
            Assert.AreEqual(2, _fixture.Board.ReadyCount);
        }

        [Test]
        public void Record_AlreadyDone_StaysAtTarget()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.Record(ContractFixture.Report(flushOuts: 1));

            _fixture.Board.Record(ContractFixture.Report(flushOuts: 2));

            Assert.AreEqual(1, _fixture.Board.Slots[1].Progress);
            Assert.IsEmpty(_fixture.Board.JustCompleted);
        }

        [Test]
        public void Record_Hunt_Saves()
        {
            _fixture.OpenFirstBoard();

            _fixture.Board.Record(ContractFixture.Report(true));

            Assert.AreEqual(1, _fixture.Repository.Load().Contracts[0].Progress);
        }

        [Test]
        public void Claim_Done_PaysOnceAndMarksClaimed()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.Record(ContractFixture.Report(flushOuts: 1));

            var paid = _fixture.Board.Claim(1);
            var again = _fixture.Board.Claim(1);

            Assert.AreEqual(ContractFixture.MediumReward, paid);
            Assert.AreEqual(-1, again);
            Assert.AreEqual(ContractFixture.MediumReward, _fixture.Store.Progress.Ectoplasm.Value);
            Assert.IsTrue(_fixture.Board.Slots[1].IsClaimed);
            Assert.AreEqual(0, _fixture.Board.ReadyCount);
        }

        [Test]
        public void Claim_NotDone_PaysNothing()
        {
            _fixture.OpenFirstBoard();

            Assert.AreEqual(-1, _fixture.Board.Claim(0));
            Assert.AreEqual(0, _fixture.Store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void ReplaceFree_FirstOfTheDay_SwapsForAnotherOfTheSameTier()
        {
            _fixture.OpenFirstBoard();

            var replaced = _fixture.Board.ReplaceFree(0);

            Assert.IsTrue(replaced);
            Assert.AreSame(_fixture.Pickups, _fixture.Board.Slots[0].Data);
            Assert.IsFalse(_fixture.Board.HasFreeReplace);
        }

        [Test]
        public void ReplaceFree_SecondTime_IsRefused()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.ReplaceFree(0);

            Assert.IsFalse(_fixture.Board.ReplaceFree(1));
            Assert.AreSame(_fixture.FlushOut, _fixture.Board.Slots[1].Data);
        }

        [Test]
        public void ReplaceFree_DoneContract_IsRefused()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.Record(ContractFixture.Report(flushOuts: 1));

            Assert.IsFalse(_fixture.Board.ReplaceFree(1));
            Assert.IsTrue(_fixture.Board.HasFreeReplace);
        }

        [Test]
        public void ReplaceFree_NextDay_IsAvailableAgain()
        {
            _fixture.OpenFirstBoard();
            _fixture.Board.ReplaceFree(0);
            _fixture.Clock.Today = _fixture.Clock.Today.AddDays(1);

            _fixture.Board.Refresh();

            Assert.IsTrue(_fixture.Board.HasFreeReplace);
        }

        [Test]
        public void ReplaceWithAd_AdWatched_SwapsOnce()
        {
            _fixture.OpenFirstBoard();

            var replaced = _fixture.Board.ReplaceWithAdAsync(0, CancellationToken.None).GetAwaiter().GetResult();
            var again = _fixture.Board.ReplaceWithAdAsync(1, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(replaced);
            Assert.IsFalse(again);
            Assert.AreSame(_fixture.Pickups, _fixture.Board.Slots[0].Data);
            Assert.AreEqual(1, _fixture.Ads.RewardedShown);
        }

        [Test]
        public void ReplaceWithAd_AdSkipped_KeepsTheContract()
        {
            _fixture.OpenFirstBoard();
            _fixture.Ads.RewardEarned = false;

            var replaced = _fixture.Board.ReplaceWithAdAsync(0, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(replaced);
            Assert.AreSame(_fixture.CaptureAny, _fixture.Board.Slots[0].Data);
            Assert.IsTrue(_fixture.Board.HasAdReplace);
        }

        [Test]
        public void Changed_Record_IsRaised()
        {
            _fixture.OpenFirstBoard();
            var raised = 0;
            Action onChanged = () => raised++;
            _fixture.Board.Changed += onChanged;

            _fixture.Board.Record(ContractFixture.Report(true));

            _fixture.Board.Changed -= onChanged;
            Assert.AreEqual(1, raised);
        }
    }
}
