using System.Threading;
using Hauntscope.Core.Services;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PaidStoreTests
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
        public void Start_RegistersEveryProductWithItsKind()
        {
            var products = _fixture.Iap.Products;

            Assert.AreEqual(5, products.Count);
            Assert.IsFalse(products.Find(p => p.Id == "full_version").IsConsumable);
            Assert.IsFalse(products.Find(p => p.Id == "starter_pack").IsConsumable);
            Assert.IsTrue(products.Find(p => p.Id == "ecto_vial").IsConsumable);
            Assert.IsTrue(products.Find(p => p.Id == "field_kit").IsConsumable);
        }

        [Test]
        public void BuyAsync_EctoplasmPack_AddsEctoplasm()
        {
            var status = _fixture.Paid.BuyAsync(_fixture.Vial, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(IapPurchaseStatus.Purchased, status);
            Assert.AreEqual(IapFixture.VialEctoplasm, _fixture.Store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void BuyAsync_FullVersion_GrantsPremiumEctoplasmAndEquipsItsLaser()
        {
            _fixture.Paid.BuyAsync(_fixture.FullVersion, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(_fixture.Paid.IsPremium);
            Assert.IsTrue(_fixture.Paid.IsOwned(_fixture.FullVersion));
            Assert.AreEqual(IapFixture.FullVersionEctoplasm, _fixture.Store.Progress.Ectoplasm.Value);
            Assert.AreEqual(_fixture.Aurum.Id, _fixture.Store.Inventory.EquippedLaserId.Value);
        }

        [Test]
        public void BuyAsync_StarterPack_GrantsGearLaserAndEctoplasm()
        {
            _fixture.Paid.BuyAsync(_fixture.StarterPack, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(IapFixture.StarterEctoplasm, _fixture.Store.Progress.Ectoplasm.Value);
            Assert.AreEqual(3, _fixture.Store.Inventory.GetCount(StoreFixture.AmpId));
            Assert.AreEqual(3, _fixture.Store.Inventory.GetCount(StoreFixture.BatteryId));
            Assert.IsTrue(_fixture.Store.Inventory.OwnsLaser(StoreFixture.FloodlightId));
        }

        [Test]
        public void Deliver_SameTransactionTwice_GrantsOnce()
        {
            _fixture.Iap.Deliver("ecto_vial", "order-1");

            var accepted = _fixture.Iap.Deliver("ecto_vial", "order-1");

            Assert.IsTrue(accepted);
            Assert.AreEqual(IapFixture.VialEctoplasm, _fixture.Store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void Deliver_OwnedItemReportedAgain_DoesNotGrantTwice()
        {
            _fixture.Iap.Deliver("full_version", "order-1");

            _fixture.Iap.Deliver("full_version", "order-2");

            Assert.AreEqual(IapFixture.FullVersionEctoplasm, _fixture.Store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void Deliver_UnknownProduct_LeavesTheOrderOpen()
        {
            var accepted = _fixture.Iap.Deliver("removed_product", "order-1");

            Assert.IsFalse(accepted);
        }

        [Test]
        public void BuyAsync_Cancelled_GrantsNothing()
        {
            _fixture.Iap.Outcome = IapPurchaseStatus.Cancelled;

            var status = _fixture.Paid.BuyAsync(_fixture.Vial, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(IapPurchaseStatus.Cancelled, status);
            Assert.AreEqual(0, _fixture.Store.Progress.Ectoplasm.Value);
        }

        [Test]
        public void BuyAsync_LaserPack_SavesOwnershipAcrossLoads()
        {
            _fixture.Paid.BuyAsync(_fixture.SpectrePack, CancellationToken.None).GetAwaiter().GetResult();

            var reloaded = _fixture.HistoryRepository.Load();

            Assert.IsTrue(reloaded.Owns("laser_spectre"));
            Assert.IsTrue(_fixture.Store.Inventory.OwnsLaser(_fixture.Spectre.Id));
        }

        [Test]
        public void FindByLaser_PaidAndEctoplasmLasers_TellsThemApart()
        {
            Assert.AreSame(_fixture.SpectrePack, _fixture.Paid.FindByLaser(_fixture.Spectre));
            Assert.AreSame(_fixture.FullVersion, _fixture.Paid.FindByLaser(_fixture.Aurum));
            Assert.IsNull(_fixture.Paid.FindByLaser(_fixture.Store.Standard));
            Assert.IsNull(_fixture.Paid.FindByLaser(_fixture.Store.Floodlight));
        }
    }
}
