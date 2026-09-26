using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Contracts;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ContractGoalsTests
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
        public void CaptureCount_Captured_CountsOne()
        {
            Assert.AreEqual(1, new CaptureCountGoal().Count(ContractFixture.Report(true), string.Empty));
        }

        [Test]
        public void CaptureCount_Escaped_CountsNothing()
        {
            Assert.AreEqual(0, new CaptureCountGoal().Count(ContractFixture.Report(), string.Empty));
        }

        [Test]
        public void Pickups_NoId_CountsEveryPickup()
        {
            var report = ContractFixture.Report(pickups: new[] { "ecto_vial", "cursed_case", "ecto_vial" });

            Assert.AreEqual(3, new PickupsGoal(string.Empty).Count(report, string.Empty));
        }

        [Test]
        public void Pickups_WithId_CountsOnlyThatKind()
        {
            var report = ContractFixture.Report(pickups: new[] { "ecto_vial", "cursed_case", "ecto_vial" });

            Assert.AreEqual(1, new PickupsGoal("cursed_case").Count(report, string.Empty));
        }

        [Test]
        public void Photo_MinStars_CountsOnlyGoodEnoughShots()
        {
            var report = ContractFixture.Report(photoStars: new[] { 0, 1, 3, 2 });

            Assert.AreEqual(3, new PhotoGoal(1).Count(report, string.Empty));
            Assert.AreEqual(1, new PhotoGoal(3).Count(report, string.Empty));
        }

        [Test]
        public void StaggerHits_Always_CountsTheHits()
        {
            Assert.AreEqual(4, new StaggerHitsGoal().Count(ContractFixture.Report(staggerHits: 4), string.Empty));
        }

        [Test]
        public void FlushOut_Always_CountsTheFlushOuts()
        {
            Assert.AreEqual(2, new FlushOutGoal().Count(ContractFixture.Report(flushOuts: 2), string.Empty));
        }

        [Test]
        public void CloseCapture_WithinReach_CountsOne()
        {
            Assert.AreEqual(1, new CloseCaptureGoal(1f).Count(ContractFixture.Report(true, captureDistance: 0.8f), string.Empty));
        }

        [Test]
        public void CloseCapture_TooFar_CountsNothing()
        {
            Assert.AreEqual(0, new CloseCaptureGoal(1f).Count(ContractFixture.Report(true, captureDistance: 1.4f), string.Empty));
        }

        [Test]
        public void CleanCapture_LaserOnly_CountsOne()
        {
            Assert.AreEqual(1, new CleanCaptureGoal().Count(ContractFixture.Report(true), string.Empty));
        }

        [Test]
        public void CleanCapture_SpareOrBoosterUsed_CountsNothing()
        {
            var goal = new CleanCaptureGoal();

            Assert.AreEqual(0, goal.Count(ContractFixture.Report(true, usedSpare: true), string.Empty));
            Assert.AreEqual(0, goal.Count(ContractFixture.Report(true, usedBoosters: true), string.Empty));
        }

        [Test]
        public void BatteryLeft_EnoughCharge_CountsOne()
        {
            Assert.AreEqual(1, new BatteryLeftGoal(0.5f).Count(ContractFixture.Report(true, batteryLeft: 0.5f), string.Empty));
        }

        [Test]
        public void BatteryLeft_TooLittleOrEscaped_CountsNothing()
        {
            var goal = new BatteryLeftGoal(0.5f);

            Assert.AreEqual(0, goal.Count(ContractFixture.Report(true, batteryLeft: 0.4f), string.Empty));
            Assert.AreEqual(0, goal.Count(ContractFixture.Report(batteryLeft: 0.9f), string.Empty));
        }

        [Test]
        public void FastCapture_InTime_CountsOne()
        {
            Assert.AreEqual(1, new FastCaptureGoal(90f).Count(ContractFixture.Report(true, duration: 80f), string.Empty));
        }

        [Test]
        public void FastCapture_TooSlow_CountsNothing()
        {
            Assert.AreEqual(0, new FastCaptureGoal(90f).Count(ContractFixture.Report(true, duration: 95f), string.Empty));
        }

        [Test]
        public void ShiftRound_ReachedTheRound_CountsOne()
        {
            var goal = new ShiftRoundGoal(3);

            Assert.AreEqual(1, goal.Count(ContractFixture.Report(shiftRound: 3), string.Empty));
            Assert.AreEqual(0, goal.Count(ContractFixture.Report(true, shiftRound: 2), string.Empty));
        }

        [Test]
        public void ShiftRound_ShiftStillLocked_IsNotOffered()
        {
            var goal = new ShiftRoundGoal(3);

            Assert.IsFalse(goal.TrySelectSubject(Context(sessions: 2), out _));
            Assert.IsTrue(goal.TrySelectSubject(Context(sessions: 3), out _));
        }

        [Test]
        public void CaptureGhost_SeenRareGhost_IsTheSubject()
        {
            var context = Context(sighted: new[] { "wisp", "wraith", "lurker" });

            Assert.IsTrue(new CaptureGhostGoal(GhostRarity.Rare).TrySelectSubject(context, out var subject));

            Assert.AreEqual("wraith", subject);
        }

        [Test]
        public void CaptureGhost_NoRareGhostSeen_IsNotOffered()
        {
            var context = Context(sighted: new[] { "wisp", "lurker" });

            Assert.IsFalse(new CaptureGhostGoal(GhostRarity.Rare).TrySelectSubject(context, out _));
        }

        [Test]
        public void CaptureGhost_ThatGhostCaught_CountsOne()
        {
            var goal = new CaptureGhostGoal(GhostRarity.Rare);

            Assert.AreEqual(1, goal.Count(ContractFixture.Report(true, _fixture.Wraith), "wraith"));
            Assert.AreEqual(0, goal.Count(ContractFixture.Report(true, _fixture.Wisp), "wraith"));
        }

        private ContractContext Context(int sessions = 0, string[] sighted = null)
        {
            var progress = new PlayerProgress();
            for (var i = 0; i < sessions; i++)
                progress.RegisterSession();
            foreach (var id in sighted ?? new string[0])
                progress.MarkSighted(id);
            return new ContractContext(progress, _fixture.Ghosts.Ghosts, _fixture.Shift, new FakeRandom());
        }
    }
}
