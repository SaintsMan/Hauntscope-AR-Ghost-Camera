using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Shift;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class NightShiftTests
    {
        private const int Reward = 20;

        private ShiftFixture _fixture;
        private GhostData _ghost;

        private NightShift Shift => _fixture.Shift;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ShiftFixture();
            _ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(_ghost);
            serialized.FindProperty("_capture._reward").intValue = Reward;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture.Dispose();
            Object.DestroyImmediate(_ghost);
        }

        [Test]
        public void BeginRound_SingleHunt_StaysOffAndUsesBoostersUp()
        {
            var fixture = new ShiftFixture(HuntMode.Single);

            var consume = fixture.Shift.BeginRound();

            Assert.IsTrue(consume);
            Assert.AreEqual(ShiftPhase.Off, fixture.Shift.Phase.Value);
            Assert.AreEqual(1f, fixture.Shift.RewardMultiplier);
            Assert.IsNull(fixture.Shift.Weights);
            fixture.Dispose();
        }

        [Test]
        public void BeginRound_NewShift_StartsTheFirstRoundAndUsesBoostersUp()
        {
            var consume = Shift.BeginRound();

            Assert.IsTrue(consume);
            Assert.AreEqual(0, Shift.Round);
            Assert.AreEqual(ShiftPhase.Hunting, Shift.Phase.Value);
            Assert.AreEqual(1, _fixture.Store.Progress.BestShiftRound);
        }

        [Test]
        public void RecordResult_CatchBeforeTheLastRound_WaitsForTheBreak()
        {
            Shift.BeginRound();

            Shift.RecordResult(Caught());

            Assert.AreEqual(ShiftPhase.BetweenRounds, Shift.Phase.Value);
            Assert.AreEqual(Reward, Shift.Earned);
        }

        [Test]
        public void OpenBreak_BetweenRounds_OffersThreeDifferentPerks()
        {
            Shift.BeginRound();
            Shift.RecordResult(Caught());

            Shift.OpenBreak();

            Assert.AreEqual(ShiftPhase.Break, Shift.Phase.Value);
            Assert.AreEqual(3, Shift.Offer.Count);
            CollectionAssert.AllItemsAreUnique(Shift.Offer);
        }

        [Test]
        public void Choose_Perk_MovesOnWithoutUsingBoostersAgain()
        {
            Shift.BeginRound();
            Shift.RecordResult(Caught());
            Shift.OpenBreak();

            Shift.Choose(_fixture.IndexOf(_fixture.ColdLens));
            var consume = Shift.BeginRound();

            Assert.IsFalse(consume);
            Assert.AreEqual(1, Shift.Round);
            Assert.AreEqual(2, _fixture.Store.Progress.BestShiftRound);
            CollectionAssert.AreEqual(new[] { _fixture.ColdLens }, Shift.Chosen);
        }

        [Test]
        public void RewardMultiplier_LaterRounds_PaysMore()
        {
            Shift.BeginRound();
            var first = Shift.RewardMultiplier;
            NextRound(_fixture.Recharge);
            var second = Shift.RewardMultiplier;
            NextRound(_fixture.Recharge);

            Assert.AreEqual(1f, first);
            Assert.AreEqual(1.5f, second);
            Assert.AreEqual(2f, Shift.RewardMultiplier);
        }

        [Test]
        public void ApplyRound_SecondRound_GhostResistsMoreAndMovesFaster()
        {
            Shift.BeginRound();
            NextRound(_fixture.Recharge);
            _fixture.Store.Modifiers.Reset();

            Shift.ApplyRound();

            Assert.AreEqual(1f / 1.15f, _fixture.Store.Modifiers.CaptureRate, 1e-4f);
            Assert.AreEqual(1.1f, _fixture.Store.Modifiers.GhostSpeed, 1e-4f);
        }

        [Test]
        public void ApplyRound_ModifierPerk_LastsForEveryLaterRound()
        {
            Shift.BeginRound();
            NextRound(_fixture.ColdLens);
            NextRound(_fixture.Recharge);
            _fixture.Store.Modifiers.Reset();

            Shift.ApplyRound();

            Assert.AreEqual(ShiftFixture.LensDrain, _fixture.Store.Modifiers.LensDrain, 1e-4f);
        }

        [Test]
        public void Choose_ChargePerk_TopsTheBatteryUpAtOnce()
        {
            Shift.BeginRound();
            _fixture.Battery.Drain(80f);
            var before = _fixture.Battery.Normalized;

            NextRound(_fixture.Recharge);

            Assert.AreEqual(before + ShiftFixture.PerkCharge, _fixture.Battery.Normalized, 1e-4f);
        }

        [Test]
        public void ApplyRound_FilmPerk_LoadsExtraFrames()
        {
            Shift.BeginRound();
            NextRound(_fixture.Cassette);

            Shift.ApplyRound();

            Assert.AreEqual(ShiftFixture.PerkFrames, Shift.ExtraFilm);
        }

        [Test]
        public void RecordResult_LastRoundCaught_CompletesAndPaysTheBonus()
        {
            Shift.BeginRound();
            NextRound(_fixture.Recharge);
            NextRound(_fixture.Recharge);

            Shift.RecordResult(Caught());

            var boosters = _fixture.Store.Inventory.GetCount(StoreFixture.AmpId) + _fixture.Store.Inventory.GetCount(StoreFixture.SaltId);
            Assert.AreEqual(ShiftPhase.Ended, Shift.Phase.Value);
            Assert.IsTrue(Shift.IsCompleted);
            Assert.AreEqual(ShiftFixture.CompleteBonus, _fixture.Store.Progress.Ectoplasm.Value);
            Assert.AreEqual(1, boosters);
            Assert.AreEqual(1, _fixture.Store.Progress.ShiftsCompleted);
            Assert.AreEqual(Reward * 3, Shift.Earned);
        }

        [Test]
        public void RecordResult_GhostEscaped_EndsWithoutTheBonus()
        {
            Shift.BeginRound();
            NextRound(_fixture.Recharge);

            Shift.RecordResult(new HuntResult(HuntOutcome.Escaped, _ghost, 60f));

            Assert.AreEqual(ShiftPhase.Ended, Shift.Phase.Value);
            Assert.IsFalse(Shift.IsCompleted);
            Assert.AreEqual(0, _fixture.Store.Progress.Ectoplasm.Value);
            Assert.AreEqual(0, _fixture.Store.Progress.ShiftsCompleted);
            Assert.AreEqual(2, _fixture.Store.Progress.BestShiftRound);
        }

        [Test]
        public void BeginRound_AfterTheShiftEnded_StartsAFreshOne()
        {
            Shift.BeginRound();
            NextRound(_fixture.ColdLens);
            Shift.RecordResult(new HuntResult(HuntOutcome.Escaped, _ghost, 60f));

            var consume = Shift.BeginRound();

            Assert.IsTrue(consume);
            Assert.AreEqual(0, Shift.Round);
            Assert.AreEqual(0, Shift.Earned);
            Assert.IsEmpty(Shift.Chosen);
        }

        // Catches the current round's ghost, takes the break and chooses the given perk, then starts the next round.
        private void NextRound(ShiftPerkData perk)
        {
            Shift.RecordResult(Caught());
            Shift.OpenBreak();
            Shift.Choose(_fixture.IndexOf(perk));
            Shift.BeginRound();
        }

        private HuntResult Caught()
        {
            return new HuntResult(HuntOutcome.Captured, _ghost, 60f);
        }
    }
}
