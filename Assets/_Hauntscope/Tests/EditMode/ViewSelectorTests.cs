using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ViewSelectorTests
    {
        private FakeViewMode _night;
        private FakeViewMode _thermal;
        private ViewSelector _views;

        [SetUp]
        public void SetUp()
        {
            _night = new FakeViewMode { DrainPerSecond = 0.3f };
            _thermal = new FakeViewMode { DrainPerSecond = 0.8f };
            _views = new ViewSelector(new IViewMode[] { _night, _thermal });
        }

        [Test]
        public void Current_AtStart_IsThePlainPicture()
        {
            Assert.IsNull(_views.Current.Value);
            Assert.IsFalse(_views.IsActive.Value);
        }

        [Test]
        public void Cycle_FromPlain_SwitchesToTheFirstMode()
        {
            _views.Cycle();

            Assert.AreSame(_night, _views.Current.Value);
            Assert.IsTrue(_night.IsActive.Value);
        }

        [Test]
        public void Cycle_FromFirstMode_SwitchesToTheNextAndOffTheFirst()
        {
            _views.Cycle();

            _views.Cycle();

            Assert.AreSame(_thermal, _views.Current.Value);
            Assert.IsFalse(_night.IsActive.Value);
            Assert.IsTrue(_thermal.IsActive.Value);
        }

        [Test]
        public void Cycle_FromLastMode_GoesBackToPlain()
        {
            _views.Cycle();
            _views.Cycle();

            _views.Cycle();

            Assert.IsNull(_views.Current.Value);
            Assert.IsFalse(_thermal.IsActive.Value);
        }

        [Test]
        public void Cycle_ModeNotIssuedYet_SkipsIt()
        {
            _night.IsUnlocked = false;

            _views.Cycle();

            Assert.AreSame(_thermal, _views.Current.Value);
        }

        [Test]
        public void Cycle_NothingIssued_StaysPlain()
        {
            _night.IsUnlocked = false;
            _thermal.IsUnlocked = false;

            _views.Cycle();

            Assert.IsNull(_views.Current.Value);
        }

        [Test]
        public void HasUnlocked_OneModeIssued_IsTrue()
        {
            _night.IsUnlocked = false;

            Assert.IsTrue(_views.HasUnlocked);
        }

        [Test]
        public void DrainPerSecond_ModeOn_IsThatModesDrain()
        {
            _views.Cycle();
            _views.Cycle();

            Assert.AreEqual(0.8f, _views.DrainPerSecond, 1e-5f);
        }

        [Test]
        public void Deactivate_ModeOn_BackToPlainAndModeOff()
        {
            _views.Cycle();

            _views.Deactivate();

            Assert.IsNull(_views.Current.Value);
            Assert.IsFalse(_night.IsActive.Value);
            Assert.AreEqual(0f, _views.DrainPerSecond);
        }

        [Test]
        public void Tick_Always_TicksEveryMode()
        {
            _views.Tick(0.1f);

            Assert.AreEqual(1, _night.Ticks);
            Assert.AreEqual(1, _thermal.Ticks);
        }
    }
}
