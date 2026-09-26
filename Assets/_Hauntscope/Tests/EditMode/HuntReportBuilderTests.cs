using System.Threading;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Photo;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntReportBuilderTests
    {
        private const float Step = 0.01f;
        private const float HideRevealRange = 1.2f;

        private GhostFixture _ghost;
        private HuntSession _session;
        private ShiftFixture _shift;
        private ReportFixture _report;
        private GhostData _data;

        [SetUp]
        public void SetUp()
        {
            _ghost = new GhostFixture(hide: new HideConfig(1f, 20f, 8f, 8f, HideRevealRange, 1f, 2f));
            _ghost.HideSpots.SpotList.Add(new Vector3(1.5f, 0f, 1.5f));
            _ghost.Mover.Teleport(new Vector3(0f, 1.5f, 0f));
            _data = ContractFixture.Ghost("wraith", GhostRarity.Rare);
            _session = new HuntSession();
            _shift = new ShiftFixture(HuntMode.Single);
            _report = new ReportFixture(_session, _ghost, _shift);
            _ghost.Ghost.Start();
            _session.Begin(_ghost.Ghost, _data);
        }

        [TearDown]
        public void TearDown()
        {
            _report.Dispose();
            _shift.Dispose();
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void Build_BeamHoldsVulnerableGhostLongEnough_CountsOneHit()
        {
            Lock();
            _ghost.Ghost.Stagger(2f);

            _report.Builder.Tick(0.3f);
            _report.Builder.Tick(0.3f);
            _report.Builder.Tick(0.3f);

            Assert.AreEqual(1, Build().StaggerHits);
        }

        [Test]
        public void Build_HoldTooShort_CountsNoHit()
        {
            Lock();
            _ghost.Ghost.Stagger(2f);

            _report.Builder.Tick(0.3f);

            Assert.AreEqual(0, Build().StaggerHits);
        }

        [Test]
        public void Build_BeamOffTheGhost_CountsNoHit()
        {
            _ghost.Ghost.Stagger(2f);

            _report.Builder.Tick(0.3f);
            _report.Builder.Tick(0.3f);

            Assert.AreEqual(0, Build().StaggerHits);
        }

        [Test]
        public void Build_TwoVulnerableWindows_CountsTwoHits()
        {
            Lock();
            _ghost.Ghost.Stagger(0.5f);
            _report.Builder.Tick(0.6f);
            _ghost.Ghost.SetReveal(0f);
            _ghost.Ghost.Tick(0.6f);
            _report.Builder.Tick(Step);
            Lock();
            _ghost.Ghost.Stagger(2f);

            _report.Builder.Tick(0.6f);

            Assert.AreEqual(2, Build().StaggerHits);
        }

        [Test]
        public void Build_PhotoTaken_ReportsItsStars()
        {
            var stars = -1;
            _report.Camera.PhotoTaken += shot => stars = shot.Score.Stars;
            _report.Camera.BeginHunt();
            _ghost.Ghost.SetReveal(1f);
            _report.Camera.Tick(Step);

            _report.Camera.ShootAsync(CancellationToken.None).GetAwaiter().GetResult();

            CollectionAssert.AreEqual(new[] { stars }, Build().PhotoStars);
        }

        [Test]
        public void Build_PickupsCollected_ListsThem()
        {
            _report.Loot.RecordPickup("ecto_vial");
            _report.Loot.RecordPickup("cursed_case");

            var report = Build();

            Assert.AreEqual(2, report.CountPickups(string.Empty));
            Assert.AreEqual(1, report.CountPickups("cursed_case"));
        }

        [Test]
        public void Build_SpareBatteryUsed_ReportsIt()
        {
            _shift.Store.Inventory.AddGear(StoreFixture.BatteryId, 1);
            _report.Battery.Drain(50f);

            _report.Spares.TryUse();

            Assert.IsTrue(Build().UsedSpareBattery);
        }

        [Test]
        public void Build_NextHunt_StartsFromScratch()
        {
            _shift.Store.Inventory.AddGear(StoreFixture.BatteryId, 1);
            _report.Battery.Drain(50f);
            _report.Spares.TryUse();

            _session.Reset();
            var next = new GhostFixture();
            next.Ghost.Start();
            _session.Begin(next.Ghost, _data);

            Assert.IsFalse(Build().UsedSpareBattery);
        }

        [Test]
        public void Build_BoostersTaken_ReportsThem()
        {
            _shift.Store.Inventory.AddGear(StoreFixture.SaltId, 1);
            _shift.Store.Inventory.SetArmed(StoreFixture.SaltId, true);

            _report.Loadout.Begin();

            Assert.IsTrue(Build().UsedBoosters);
        }

        [Test]
        public void Build_GhostDrivenOutOfHiding_CountsFlushOut()
        {
            _ghost.Ghost.SetReveal(1f);
            _ghost.Ghost.Tick(Step);
            _ghost.Ghost.SetReveal(0f);
            _ghost.Ghost.Tick(GhostFixture.AlertedPause + Step);
            _ghost.Ghost.Tick(Step);
            _ghost.Ghost.SetReveal(1f);

            _ghost.Ghost.Tick(Step);
            _ghost.Ghost.Tick(Step);

            Assert.AreEqual(1, Build().FlushOuts);
        }

        [Test]
        public void Build_Captured_ReportsTheDistance()
        {
            Lock();

            var report = Build(HuntOutcome.Captured);

            Assert.AreEqual(1.5f, report.CaptureDistance, 1e-3f);
            Assert.IsTrue(report.IsCaptured);
        }

        [Test]
        public void Build_Escaped_IsNotACloseCapture()
        {
            Lock();

            var report = Build();

            Assert.AreEqual(float.MaxValue, report.CaptureDistance);
            Assert.IsFalse(report.IsCaptured);
        }

        [Test]
        public void Build_Always_ReportsTheBatteryLeft()
        {
            _report.Battery.Drain(30f);

            Assert.AreEqual(_report.Battery.Normalized, Build().BatteryLeft, 1e-4f);
        }

        [Test]
        public void Build_OutsideAShift_ReportsNoRound()
        {
            Assert.AreEqual(0, Build().ShiftRound);
        }

        [Test]
        public void Build_ShiftRound_ReportsWhichOne()
        {
            _report.Dispose();
            _shift.Dispose();
            _shift = new ShiftFixture();
            _report = new ReportFixture(_session, _ghost, _shift);
            _session.Reset();
            _shift.Shift.BeginRound();
            var ghost = new GhostFixture();
            ghost.Ghost.Start();

            _session.Begin(ghost.Ghost, _data);

            Assert.AreEqual(1, Build().ShiftRound);
        }

        private void Lock()
        {
            _ghost.Ghost.SetReveal(1f);
            _report.Beam.Activate();
            _report.Beam.Tick(Step);
        }

        private HuntReport Build(HuntOutcome outcome = HuntOutcome.Escaped)
        {
            _session.Finish(outcome);
            return _report.Builder.Build(_session.Result.Value);
        }
    }
}
