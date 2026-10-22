using System;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Season
{
    // Makes the save fit a promo take when the developer switches ask for a quiet one: the tutorial counts as done and
    // every field tip and coach mark as seen, so nothing but the event itself is on screen.
    public sealed class DemoPrep : IInitializable
    {
        private readonly DemoSwitches _demo;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly MenuCoachMarks _marks;

        public DemoPrep(DemoSwitches demo, PlayerProgress progress, PlayerProgressRepository repository, MenuCoachMarks marks)
        {
            _demo = demo;
            _progress = progress;
            _repository = repository;
            _marks = marks;
        }

        public void Initialize()
        {
            if (!_demo.IsOn || !_demo.Quiet)
                return;

            _progress.MarkTutorialCompleted();
            foreach (FieldTipId tip in Enum.GetValues(typeof(FieldTipId)))
                _progress.MarkTipSeen(FieldTips.KeyOf(tip));
            foreach (CoachMark mark in Enum.GetValues(typeof(CoachMark)))
                _marks.Dismiss(mark);
            _repository.Save(_progress);
        }
    }
}
