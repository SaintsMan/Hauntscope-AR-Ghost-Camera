using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.Tips;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ViewModeTipRuleTests
    {
        private FieldTipContext _context;
        private FakeViewMode _mode;
        private ViewModeTipRule _rule;

        [SetUp]
        public void SetUp()
        {
            var fixture = new GhostFixture();
            var session = new HuntSession();
            session.Begin(fixture.Ghost, null);
            _context = new FieldTipContext(session, null, null, null, null, null, fixture.Camera, null);
            _mode = new FakeViewMode();
            _rule = new ViewModeTipRule(FieldTipId.NightVision, _mode);
        }

        [Test]
        public void Id_Always_IsTheOneItWasGiven()
        {
            Assert.AreEqual(FieldTipId.NightVision, _rule.Id);
        }

        [Test]
        public void IsDue_ModeIssued_IsTrue()
        {
            Assert.IsTrue(_rule.IsDue(_context, 0.1f));
        }

        [Test]
        public void IsDue_ModeNotIssued_IsFalse()
        {
            _mode.IsUnlocked = false;

            Assert.IsFalse(_rule.IsDue(_context, 0.1f));
        }

        [Test]
        public void IsResolved_PlayerSwitchedToTheMode_IsTrue()
        {
            _mode.Activate();

            Assert.IsTrue(_rule.IsResolved(_context));
        }
    }
}
