using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostHideStateTests
    {
        private const float Cooldown = 20f;
        private const float Duration = 8f;
        private const float HideRevealRange = 1.2f;
        private const float FlushStagger = 1f;
        private const float MinDistance = 2f;
        private const float Step = 0.01f;

        private static readonly Vector3 NearPlayer = new Vector3(0f, 0f, -1.5f);
        private static readonly Vector3 FarCorner = new Vector3(1.5f, 0f, 1.5f);

        [Test]
        public void Retarget_NeverAlerted_KeepsWandering()
        {
            var fixture = Create();
            fixture.Ghost.Start();

            fixture.Ghost.Tick(4f + Step);
            fixture.Ghost.Tick(Step);

            Assert.IsFalse(fixture.Ghost.IsHiding);
        }

        [Test]
        public void Retarget_AfterAlert_Hides()
        {
            var fixture = Create();

            Alert(fixture.Ghost);
            fixture.Ghost.Tick(Step);

            Assert.IsTrue(fixture.Ghost.IsHiding);
        }

        [Test]
        public void Retarget_ChanceMissed_StaysAlerted()
        {
            var fixture = Create(chance: 0f);

            Alert(fixture.Ghost);
            fixture.Ghost.Tick(Step);

            Assert.IsFalse(fixture.Ghost.IsHiding);
            Assert.IsTrue(fixture.Ghost.IsAlerted);
        }

        [Test]
        public void Retarget_NoHideSpots_StaysAlerted()
        {
            var fixture = Create();
            fixture.HideSpots.SpotList.Clear();

            Alert(fixture.Ghost);
            fixture.Ghost.Tick(Step);

            Assert.IsFalse(fixture.Ghost.IsHiding);
        }

        [Test]
        public void Retarget_GhostCannotHide_StaysAlerted()
        {
            var fixture = new GhostFixture();
            fixture.HideSpots.SpotList.Add(FarCorner);

            Alert(fixture.Ghost);
            fixture.Ghost.Tick(Step);

            Assert.IsFalse(fixture.Ghost.IsHiding);
        }

        [Test]
        public void Retarget_StillInTheLens_StaysAlerted()
        {
            var fixture = Create();
            fixture.Ghost.Start();
            fixture.Ghost.SetReveal(1f);

            fixture.Ghost.Tick(Step);
            fixture.Ghost.Tick(GhostFixture.AlertedPause + Step);
            fixture.Ghost.Tick(Step);

            Assert.IsFalse(fixture.Ghost.IsHiding);
        }

        [Test]
        public void Hide_FarSpotAvailable_SkipsSpotNearPlayer()
        {
            var fixture = Create();

            Hide(fixture.Ghost);

            Assert.AreEqual(FarCorner, fixture.Ghost.HideSpot);
        }

        [Test]
        public void Hide_EverySpotClose_PicksFarthest()
        {
            var fixture = Create();
            var closer = new Vector3(0.5f, 0f, -0.5f);
            fixture.HideSpots.SpotList.Clear();
            fixture.HideSpots.SpotList.Add(NearPlayer);
            fixture.HideSpots.SpotList.Add(closer);

            Hide(fixture.Ghost);

            Assert.AreEqual(closer, fixture.Ghost.HideSpot);
        }

        [Test]
        public void Hide_Started_CrouchesJustAboveTheFloor()
        {
            var fixture = Create();
            var config = new HideConfig();
            var bodyBottom = new GhostMotion().BodyBottom;
            var clearance = Mathf.Lerp(config.FloorClearanceMin, config.FloorClearanceMax, fixture.Random.DefaultValue);

            Hide(fixture.Ghost);

            var expected = new Vector3(FarCorner.x, -bodyBottom + clearance, FarCorner.z);
            Assert.That(Vector3.Distance(expected, fixture.Mover.Target), Is.LessThan(1e-4f));
        }

        [Test]
        public void Hide_Started_ReportsSpot()
        {
            var fixture = Create();
            Vector3? started = null;
            fixture.Ghost.HideStarted += spot => started = spot;

            Hide(fixture.Ghost);

            Assert.AreEqual(FarCorner, started);
        }

        [Test]
        public void Hide_WhileHiding_ShortensRevealRange()
        {
            var fixture = Create();

            Hide(fixture.Ghost);

            Assert.AreEqual(HideRevealRange, fixture.Ghost.RevealRange);
        }

        [Test]
        public void Hide_RevealedByLens_FlushesStaggeredThenFlees()
        {
            var fixture = Create();
            var flushed = false;
            fixture.Ghost.Flushed += () => flushed = true;
            Hide(fixture.Ghost);
            fixture.Ghost.SetReveal(1f);

            fixture.Ghost.Tick(Step);
            fixture.Ghost.Tick(Step);

            Assert.IsTrue(flushed);
            Assert.IsTrue(fixture.Ghost.IsStaggered);
            Assert.IsTrue(fixture.Ghost.IsFleeing);
        }

        [Test]
        public void Hide_TimeRunsOut_ReturnsToWanderWithFullRevealRange()
        {
            var fixture = Create();
            var ended = false;
            fixture.Ghost.HideEnded += () => ended = true;
            Hide(fixture.Ghost);

            fixture.Ghost.Tick(Duration + Step);
            fixture.Ghost.Tick(Step);

            Assert.IsTrue(ended);
            Assert.IsFalse(fixture.Ghost.IsHiding);
            Assert.IsFalse(fixture.Ghost.IsAlerted);
            Assert.AreEqual(GhostFixture.RevealRange, fixture.Ghost.RevealRange);
        }

        [Test]
        public void Hide_Captured_EndsHide()
        {
            var fixture = Create();
            var ended = false;
            fixture.Ghost.HideEnded += () => ended = true;
            Hide(fixture.Ghost);

            fixture.Ghost.Capture();
            fixture.Ghost.Tick(Step);

            Assert.IsTrue(ended);
            Assert.IsTrue(fixture.Ghost.IsCaptured);
        }

        [Test]
        public void HideEnded_WithinCooldown_DoesNotHideAgain()
        {
            var fixture = Create();
            Hide(fixture.Ghost);
            fixture.Ghost.Tick(Duration + Step);

            for (var elapsed = 0f; elapsed < Cooldown - 5f; elapsed += 4f + Step)
                fixture.Ghost.Tick(4f + Step);
            fixture.Ghost.Tick(Step);

            Assert.IsFalse(fixture.Ghost.IsHiding);
        }

        [Test]
        public void HideEnded_AfterCooldown_HidesAgain()
        {
            var fixture = Create();
            Hide(fixture.Ghost);
            fixture.Ghost.Tick(Duration + Step);

            for (var elapsed = 0f; elapsed < Cooldown + 4f; elapsed += 4f + Step)
                fixture.Ghost.Tick(4f + Step);
            fixture.Ghost.Tick(Step);

            Assert.IsTrue(fixture.Ghost.IsHiding);
        }

        private static GhostFixture Create(float chance = 1f)
        {
            var fixture = new GhostFixture(hide: new HideConfig(chance, Cooldown, Duration, Duration, HideRevealRange, FlushStagger, MinDistance));
            fixture.HideSpots.SpotList.Add(NearPlayer);
            fixture.HideSpots.SpotList.Add(FarCorner);
            return fixture;
        }

        // Spotted in the lens, then left alone: when the alerted pause ends the ghost picks a new target and decides.
        private static void Alert(Ghost ghost)
        {
            ghost.Start();
            ghost.SetReveal(1f);
            ghost.Tick(Step);
            ghost.SetReveal(0f);
            ghost.Tick(GhostFixture.AlertedPause + Step);
        }

        private static void Hide(Ghost ghost)
        {
            Alert(ghost);
            ghost.Tick(Step);
        }
    }
}
