using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ColdSpotFeedbackTests
    {
        private const float Duration = 8f;
        private const float Step = 0.01f;

        private static readonly Vector3 Spot = new Vector3(1.5f, 0f, 1.5f);

        private GhostFixture _fixture;
        private HuntSession _session;
        private FakeVfxPlayer _vfx;
        private ColdSpotFeedback _feedback;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture(hide: new HideConfig(1f, 20f, Duration, Duration, 1.2f, 1f, 2f));
            _fixture.HideSpots.SpotList.Add(Spot);
            _session = new HuntSession();
            _vfx = new FakeVfxPlayer();
            _feedback = new ColdSpotFeedback(_session, _vfx, new VfxConfig());
            _feedback.Start();
            _session.Begin(_fixture.Ghost, null);
        }

        [TearDown]
        public void TearDown()
        {
            _feedback.Dispose();
        }

        [Test]
        public void HideStarted_Always_PlaysColdSpotAtSpot()
        {
            Hide();

            Assert.AreEqual(1, _vfx.Loops.Count);
            Assert.AreEqual(VfxId.ColdSpot, _vfx.Loops[0].Id);
            Assert.AreEqual(Spot, _vfx.Loops[0].Position);
        }

        [Test]
        public void HideEnded_TimeRunsOut_StopsColdSpot()
        {
            Hide();

            _fixture.Ghost.Tick(Duration + Step);
            _fixture.Ghost.Tick(Step);

            Assert.IsTrue(_vfx.Loops[0].IsStopped);
        }

        [Test]
        public void GhostReplaced_WhileHiding_StopsColdSpot()
        {
            Hide();

            _session.Begin(new GhostFixture().Ghost, null);

            Assert.IsTrue(_vfx.Loops[0].IsStopped);
        }

        private void Hide()
        {
            var ghost = _fixture.Ghost;
            ghost.Start();
            ghost.SetReveal(1f);
            ghost.Tick(Step);
            ghost.SetReveal(0f);
            ghost.Tick(GhostFixture.AlertedPause + Step);
            ghost.Tick(Step);
        }
    }
}
