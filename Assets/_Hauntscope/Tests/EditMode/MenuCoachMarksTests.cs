using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class MenuCoachMarksTests
    {
        private PlayerProgress _progress;
        private PlayerProgressRepository _repository;
        private MenuCoachMarks _marks;

        [SetUp]
        public void SetUp()
        {
            var features = new FeatureFactory();
            _progress = features.Progress;
            _repository = features.Repository;
            _marks = new MenuCoachMarks(_progress, _repository, features.Features);
        }

        [Test]
        public void IsMarked_BeforeTheFirstHunt_NothingIsMarked()
        {
            Assert.IsFalse(_marks.IsMarked(CoachMark.Contracts));
            Assert.IsFalse(_marks.IsMarked(CoachMark.Shift));
        }

        [Test]
        public void IsMarked_AfterTheFirstHunt_MarksContracts()
        {
            _progress.RegisterSession();

            Assert.IsTrue(_marks.IsMarked(CoachMark.Contracts));
            Assert.IsFalse(_marks.IsMarked(CoachMark.Shift));
        }

        [Test]
        public void IsMarked_ShiftOpened_MarksTheShift()
        {
            Play(new ShiftConfig().UnlockHunts);

            Assert.IsTrue(_marks.IsMarked(CoachMark.Shift));
        }

        [Test]
        public void Dismiss_Marked_GoesForGoodAndSaves()
        {
            _progress.RegisterSession();

            _marks.Dismiss(CoachMark.Contracts);

            Assert.IsFalse(_marks.IsMarked(CoachMark.Contracts));
            Assert.IsTrue(_repository.Load().HasSeenTip("coach.contracts"));
        }

        [Test]
        public void Dismiss_NotYetMarked_KeepsItForLater()
        {
            _marks.Dismiss(CoachMark.Shift);
            Play(new ShiftConfig().UnlockHunts);

            Assert.IsTrue(_marks.IsMarked(CoachMark.Shift));
        }

        [Test]
        public void IsMarked_TipsReset_ComesBack()
        {
            _progress.RegisterSession();
            _marks.Dismiss(CoachMark.Contracts);

            _progress.ResetTips();

            Assert.IsTrue(_marks.IsMarked(CoachMark.Contracts));
        }

        [Test]
        public void IsMarked_HuntWithoutACatch_BestiaryStaysQuiet()
        {
            _progress.RegisterSession();

            Assert.IsFalse(_marks.IsMarked(CoachMark.Bestiary));
        }

        [Test]
        public void IsMarked_OneHuntLessThanNeeded_ShopStaysQuiet()
        {
            Play(new OnboardingConfig().ShopHunts - 1);

            Assert.IsFalse(_marks.IsMarked(CoachMark.Shop));
        }

        [Test]
        public void IsMarked_FirstCatch_MarksTheBestiary()
        {
            _progress.RegisterSession();

            _progress.AddCapture("wisp", 10);

            Assert.IsTrue(_marks.IsMarked(CoachMark.Bestiary));
        }

        [Test]
        public void IsMarked_AFewHuntsPlayed_MarksTheShop()
        {
            Play(new OnboardingConfig().ShopHunts);

            Assert.IsTrue(_marks.IsMarked(CoachMark.Shop));
        }

        private void Play(int hunts)
        {
            for (var i = 0; i < hunts; i++)
                _progress.RegisterSession();
        }
    }
}
