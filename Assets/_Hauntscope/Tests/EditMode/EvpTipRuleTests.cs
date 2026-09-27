using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.Tips;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class EvpTipRuleTests
    {
        private GhostFixture _fixture;
        private HuntSession _session;
        private FieldTipContext _context;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, null);
            _context = new FieldTipContext(_session, null, null, null, null, null, _fixture.Camera, null);
        }

        [Test]
        public void IsDue_RecorderIssued_IsTrue()
        {
            var rule = new EvpTipRule(Recorder(captures: 1));

            Assert.IsTrue(rule.IsDue(_context, 0.1f));
        }

        [Test]
        public void IsDue_RecorderNotIssuedYet_IsFalse()
        {
            var rule = new EvpTipRule(Recorder(captures: 0));

            Assert.IsFalse(rule.IsDue(_context, 0.1f));
        }

        [Test]
        public void IsResolved_FirstTakeStarted_IsTrue()
        {
            var recorder = Recorder(captures: 1);
            var rule = new EvpTipRule(recorder);

            recorder.Activate();

            Assert.IsTrue(rule.IsResolved(_context));
        }

        [Test]
        public void IsResolved_NoTakeYet_IsFalse()
        {
            var rule = new EvpTipRule(Recorder(captures: 1));

            Assert.IsFalse(rule.IsResolved(_context));
        }

        private EvpRecorder Recorder(int captures)
        {
            var progress = new PlayerProgress();
            for (var i = 0; i < captures; i++)
                progress.AddCapture("wisp", 10);
            return TestConfigs.EvpTool(_session, _fixture.Camera, progress: progress, config: TestConfigs.Evp(unlockCaptures: 1));
        }
    }
}
