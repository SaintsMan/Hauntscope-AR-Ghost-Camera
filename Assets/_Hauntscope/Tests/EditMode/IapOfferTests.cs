using System;
using System.Threading;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class IapOfferTests
    {
        private IapFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new IapFixture();
            _fixture.Paid.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture.Paid.Dispose();
        }

        [Test]
        public void StarterRefresh_BeforeEnoughHunts_StaysClosed()
        {
            _fixture.PlayHunts(IapFixture.StarterAfterHunts - 1);

            _fixture.Starter.Refresh();

            Assert.IsFalse(_fixture.Starter.IsActive);
        }

        [Test]
        public void StarterRefresh_AfterEnoughHunts_OpensForConfiguredHours()
        {
            _fixture.PlayHunts(IapFixture.StarterAfterHunts);

            _fixture.Starter.Refresh();

            Assert.IsTrue(_fixture.Starter.IsActive);
            Assert.AreEqual(TimeSpan.FromHours(IapFixture.StarterHours), _fixture.Starter.Remaining);
        }

        [Test]
        public void StarterIsActive_AfterTimeRunsOut_IsFalse()
        {
            _fixture.PlayHunts(IapFixture.StarterAfterHunts);
            _fixture.Starter.Refresh();

            _fixture.Clock.Advance(TimeSpan.FromHours(IapFixture.StarterHours + 1));

            Assert.IsFalse(_fixture.Starter.IsActive);
            Assert.AreEqual(TimeSpan.Zero, _fixture.Starter.Remaining);
        }

        [Test]
        public void StarterIsActive_AfterBuyingIt_IsFalse()
        {
            _fixture.PlayHunts(IapFixture.StarterAfterHunts);
            _fixture.Starter.Refresh();

            _fixture.Paid.BuyAsync(_fixture.StarterPack, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_fixture.Starter.IsActive);
        }

        [Test]
        public void StarterShouldAnnounce_OnlyUntilMarked()
        {
            _fixture.PlayHunts(IapFixture.StarterAfterHunts);
            _fixture.Starter.Refresh();
            var before = _fixture.Starter.ShouldAnnounce;

            _fixture.Starter.MarkAnnounced();

            Assert.IsTrue(before);
            Assert.IsFalse(_fixture.Starter.ShouldAnnounce);
            Assert.IsTrue(_fixture.HistoryRepository.Load().StarterOfferAnnounced);
        }

        [Test]
        public void PremiumShouldShow_NewPlayer_IsFalse()
        {
            Assert.IsFalse(_fixture.Premium.ShouldShow);
        }

        [Test]
        public void PremiumShouldShow_RegularPlayer_ShowsThenWaitsDays()
        {
            _fixture.PlayHunts(IapFixture.PremiumAfterHunts);
            _fixture.Starter.Refresh();
            _fixture.Starter.MarkAnnounced();
            var first = _fixture.Premium.ShouldShow;

            _fixture.Premium.MarkShown();
            var sameDay = _fixture.Premium.ShouldShow;
            _fixture.Clock.Today = _fixture.Clock.Today.AddDays(IapFixture.PremiumEveryDays);

            Assert.IsTrue(first);
            Assert.IsFalse(sameDay);
            Assert.IsTrue(_fixture.Premium.ShouldShow);
        }

        [Test]
        public void PremiumShouldShow_WhileStarterIsAnnounced_WaitsForIt()
        {
            _fixture.PlayHunts(IapFixture.PremiumAfterHunts);

            _fixture.Starter.Refresh();

            Assert.IsFalse(_fixture.Premium.ShouldShow);
        }

        [Test]
        public void PremiumShouldShow_AfterBuyingFullVersion_IsFalse()
        {
            _fixture.PlayHunts(IapFixture.PremiumAfterHunts);

            _fixture.Paid.BuyAsync(_fixture.FullVersion, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_fixture.Premium.ShouldShow);
        }
    }
}
