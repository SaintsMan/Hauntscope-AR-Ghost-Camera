using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.Tips;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SpiritBoxTipRuleTests
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
        public void IsDue_BoxIssued_IsTrue()
        {
            var rule = new SpiritBoxTipRule(Box(captures: 1));

            Assert.IsTrue(rule.IsDue(_context, 0.1f));
        }

        [Test]
        public void IsDue_BoxNotIssuedYet_IsFalse()
        {
            var rule = new SpiritBoxTipRule(Box(captures: 0));

            Assert.IsFalse(rule.IsDue(_context, 0.1f));
        }

        [Test]
        public void IsResolved_PlayerSwitchedTheBoxOn_IsTrue()
        {
            var box = Box(captures: 1);
            var rule = new SpiritBoxTipRule(box);

            box.Activate();

            Assert.IsTrue(rule.IsResolved(_context));
        }

        private SpiritBox Box(int captures)
        {
            var progress = new PlayerProgress();
            for (var i = 0; i < captures; i++)
                progress.AddCapture("wisp", 10);
            return TestConfigs.SpiritBoxTool(_session, _fixture.Camera, TestConfigs.Tools(), new FakeRandom(), progress,
                TestConfigs.SpiritBox(unlockCaptures: 1));
        }
    }
}
