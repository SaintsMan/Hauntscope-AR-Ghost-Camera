using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostSurgeStateTests
    {
        private const float Threshold = 0.85f;
        private const float Rearm = 0.6f;
        private const float Duration = 1.6f;
        private const float Jerk = 0.3f;

        private GhostFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = CreateFixture();
        }

        [Test]
        public void Tick_FleeingPastThreshold_StartsSurge()
        {
            MakeFleeing();
            _fixture.Ghost.SetCaptureProgress(Threshold);

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsSurging);
        }

        [Test]
        public void Tick_SurgeStarts_RaisesEvent()
        {
            var raised = 0;
            _fixture.Ghost.SurgeStarted += () => raised++;
            MakeFleeing();
            _fixture.Ghost.SetCaptureProgress(Threshold);

            _fixture.Ghost.Tick(0.01f);

            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Tick_BelowThreshold_KeepsFleeing()
        {
            MakeFleeing();
            _fixture.Ghost.SetCaptureProgress(Threshold - 0.01f);

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsFleeing);
        }

        [Test]
        public void Tick_SurgeOutlasted_ReturnsToFleeing()
        {
            StartSurge();

            _fixture.Ghost.Tick(Duration);
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsFleeing);
        }

        [Test]
        public void Tick_SurgeSpentAndProgressStillHigh_DoesNotSurgeAgain()
        {
            StartSurge();
            _fixture.Ghost.Tick(Duration);
            _fixture.Ghost.Tick(0.01f);

            _fixture.Ghost.Tick(0.01f);

            Assert.IsFalse(_fixture.Ghost.IsSurging);
        }

        [Test]
        public void Tick_ProgressDroppedBelowRearm_SurgesAgainNextTime()
        {
            StartSurge();
            _fixture.Ghost.Tick(Duration);
            _fixture.Ghost.Tick(0.01f);
            _fixture.Ghost.SetCaptureProgress(Rearm - 0.1f);

            _fixture.Ghost.SetCaptureProgress(Threshold);
            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsSurging);
        }

        [Test]
        public void Tick_Surging_StaysWithinJerkOfWhereItStarted()
        {
            MakeFleeing();
            _fixture.Ghost.SetCaptureProgress(Threshold);
            _fixture.Ghost.Tick(0.01f);
            var anchor = _fixture.Ghost.Position;
            _fixture.Random.DefaultValue = 1f;

            for (var i = 0; i < 10; i++)
                _fixture.Ghost.Tick(0.1f);

            Assert.LessOrEqual(Vector3.Distance(anchor, _fixture.Ghost.Position), Jerk * Mathf.Sqrt(2f) + 1e-3f);
            Assert.Greater(Vector3.Distance(anchor, _fixture.Ghost.Position), 0f);
        }

        [Test]
        public void Tick_Surging_KeepsAbilitiesSilent()
        {
            var ability = new CountingAbility();
            _fixture = CreateFixture(ability);
            StartSurge();
            var before = ability.Ticks;

            _fixture.Ghost.Tick(0.1f);

            Assert.AreEqual(before, ability.Ticks);
        }

        [Test]
        public void Tick_SurgeStartsWhileFlickeredOut_ShowsTheGhost()
        {
            MakeFleeing();
            _fixture.Ghost.SetVisible(false);
            _fixture.Ghost.SetCaptureProgress(Threshold);

            _fixture.Ghost.Tick(0.01f);
            _fixture.Ghost.SetVisible(false);

            Assert.IsTrue(_fixture.Ghost.IsVisible);
        }

        [Test]
        public void Tick_SurgingAndCaptured_GoesToCaptured()
        {
            StartSurge();
            _fixture.Ghost.Capture();

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsCaptured);
        }

        private static GhostFixture CreateFixture(IGhostAbility ability = null)
        {
            var capture = TestConfigs.Capture(surgeThreshold: Threshold, surgeRearm: Rearm, surgeDuration: Duration);
            var abilities = ability != null ? new[] { ability } : null;
            var fixture = new GhostFixture(capture: capture, abilities: abilities);
            fixture.Planes.RoomBounds = new Bounds(Vector3.zero, new Vector3(10f, 0f, 10f));
            fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            fixture.Ghost.Start();
            return fixture;
        }

        private void StartSurge()
        {
            MakeFleeing();
            _fixture.Ghost.SetCaptureProgress(Threshold);
            _fixture.Ghost.Tick(0.01f);
        }

        private void MakeFleeing()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Tick(0.01f);
            _fixture.Ghost.SetBeamed(true);
            _fixture.Ghost.Tick(0.01f);
        }

        private sealed class CountingAbility : IGhostAbility
        {
            public int Ticks { get; private set; }

            public void Tick(Ghost ghost, float deltaTime)
            {
                Ticks++;
            }
        }
    }
}
