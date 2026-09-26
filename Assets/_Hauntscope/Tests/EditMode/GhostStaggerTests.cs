using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostStaggerTests
    {
        private const float Sink = 0.15f;
        private const float BlendTime = 0.1f;

        private GhostFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture(capture: TestConfigs.Capture(staggerSink: Sink, staggerBlendTime: BlendTime));
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            _fixture.Ghost.Start();
        }

        [Test]
        public void Stagger_PositiveDuration_IsStaggered()
        {
            _fixture.Ghost.Stagger(1f);

            Assert.IsTrue(_fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Tick_PastDuration_RecoversFromStagger()
        {
            _fixture.Ghost.Stagger(1f);

            _fixture.Ghost.Tick(1.1f);

            Assert.IsFalse(_fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Stagger_WhileStaggered_RaisesEventOnce()
        {
            var raised = 0;
            _fixture.Ghost.Staggered += () => raised++;

            _fixture.Ghost.Stagger(1f);
            _fixture.Ghost.Stagger(2f);

            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Stagger_ShorterOverLonger_KeepsTheLongerWindow()
        {
            _fixture.Ghost.Stagger(2f);
            _fixture.Ghost.Stagger(0.5f);

            _fixture.Ghost.Tick(1f);

            Assert.IsTrue(_fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Stagger_AfterCapture_IsIgnored()
        {
            _fixture.Ghost.Capture();

            _fixture.Ghost.Stagger(1f);

            Assert.IsFalse(_fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Tick_StaggeredWhileFleeing_HoldsItsPlace()
        {
            MakeFleeing();
            _fixture.Ghost.Stagger(5f);
            var before = _fixture.Ghost.Position;

            for (var i = 0; i < 10; i++)
                _fixture.Ghost.Tick(0.1f);

            Assert.AreEqual(before.x, _fixture.Ghost.Position.x, 1e-3f);
            Assert.AreEqual(before.z, _fixture.Ghost.Position.z, 1e-3f);
        }

        [Test]
        public void Tick_FirstReveal_StaggersForTheAlertedPause()
        {
            _fixture.Ghost.SetReveal(1f);

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsAlerted);
            Assert.IsTrue(_fixture.Ghost.IsStaggered);
        }

        [Test]
        public void Tick_Staggered_ViewSagsAndFlares()
        {
            _fixture.Ghost.Stagger(5f);

            _fixture.Ghost.Tick(BlendTime);

            Assert.AreEqual(1f, _fixture.View.Stagger, 1e-5f);
            Assert.Less(_fixture.View.Position.y, _fixture.Mover.VisualPosition.y - Sink * 0.9f);
        }

        [Test]
        public void Tick_Recovered_ViewReturnsToNormal()
        {
            _fixture.Ghost.Stagger(0.1f);
            _fixture.Ghost.Tick(BlendTime);

            _fixture.Ghost.Tick(0.2f);
            _fixture.Ghost.Tick(BlendTime);

            Assert.AreEqual(0f, _fixture.View.Stagger, 1e-5f);
        }

        private void MakeFleeing()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Tick(0.01f);
            _fixture.Ghost.SetBeamed(true);
            _fixture.Ghost.Tick(GhostFixture.AlertedPause + 0.01f);
            _fixture.Ghost.Tick(0.01f);
        }
    }
}
