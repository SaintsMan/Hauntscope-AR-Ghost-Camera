using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.States;
using Hauntscope.Gameplay.Shift;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ShiftBreakStateTests
    {
        [Test]
        public void Enter_BetweenRounds_OpensTheBreakWithPerks()
        {
            var fixture = new ShiftFixture();
            var ghost = ScriptableObject.CreateInstance<GhostData>();
            fixture.Shift.BeginRound();
            fixture.Shift.RecordResult(new HuntResult(HuntOutcome.Captured, ghost, 60f));
            var state = new ShiftBreakState(fixture.Shift);

            state.Enter();

            Assert.AreEqual(ShiftPhase.Break, fixture.Shift.Phase.Value);
            Assert.AreEqual(ShiftFixture.Length, fixture.Shift.Offer.Count);
            Object.DestroyImmediate(ghost);
            fixture.Dispose();
        }

        [Test]
        public void Enter_NoPerksInThePool_SkipsToTheNextRound()
        {
            var fixture = new ShiftFixture(perks: false);
            var ghost = ScriptableObject.CreateInstance<GhostData>();
            fixture.Shift.BeginRound();
            fixture.Shift.RecordResult(new HuntResult(HuntOutcome.Captured, ghost, 60f));

            new ShiftBreakState(fixture.Shift).Enter();

            Assert.AreEqual(ShiftPhase.Hunting, fixture.Shift.Phase.Value);
            Assert.AreEqual(1, fixture.Shift.Round);
            Object.DestroyImmediate(ghost);
            fixture.Dispose();
        }

        [Test]
        public void Enter_OutsideAShift_LeavesItOff()
        {
            var fixture = new ShiftFixture(HuntMode.Single);
            var state = new ShiftBreakState(fixture.Shift);

            state.Enter();

            Assert.AreEqual(ShiftPhase.Off, fixture.Shift.Phase.Value);
            fixture.Dispose();
        }
    }
}
