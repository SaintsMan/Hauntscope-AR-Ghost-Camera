using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.States;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ResultStateTests
    {
        private GhostFixture _fixture;
        private HuntSession _session;
        private Battery _battery;
        private Toolbelt _toolbelt;
        private ResultState _state;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            var config = TestConfigs.Tools();
            _battery = new Battery(config);
            _toolbelt = new Toolbelt(new GhostLens(_session, _fixture.Camera, config), new CaptureBeam(_session, _fixture.Camera, config));
            _state = new ResultState(_session, _battery, _toolbelt);
        }

        [Test]
        public void Enter_ToolsActive_TurnsThemOff()
        {
            _toolbelt.StartBeam();

            _state.Enter();

            Assert.AreEqual(0f, _toolbelt.TotalDrainPerSecond);
        }

        [Test]
        public void Exit_AfterHunt_RefillsBattery()
        {
            _battery.Drain(60f);

            _state.Exit();

            Assert.AreEqual(1f, _battery.Normalized);
        }

        [Test]
        public void Exit_AfterHunt_ResetsCaptureProgress()
        {
            _fixture.Ghost.SetReveal(1f);
            _toolbelt.Beam.Activate();
            _toolbelt.Beam.Tick(1f);
            Assume.That(_toolbelt.Beam.Progress.Value, Is.GreaterThan(0f));

            _state.Exit();

            Assert.AreEqual(0f, _toolbelt.Beam.Progress.Value);
        }

        [Test]
        public void Exit_AfterHunt_RemovesPreviousGhost()
        {
            _state.Exit();

            Assert.IsNull(_session.Ghost.Value);
            Assert.IsTrue(_fixture.View.IsDespawned);
        }
    }
}
