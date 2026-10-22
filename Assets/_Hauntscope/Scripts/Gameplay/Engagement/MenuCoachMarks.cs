using Hauntscope.Gameplay.Onboarding;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Engagement
{
    // One-off NEW marks in the menu (GDD 5.31, 5.41): a button that has just opened up carries one — the night shift, the
    // contracts, the bestiary, the shop (the archive shows its unheard tapes as a badge instead). A mark goes for good the
    // first time its button is pressed; "Reset tips" brings it back. When a button opens is FeatureUnlocks' rule, so the
    // console moves the marks with it.
    public sealed class MenuCoachMarks
    {
        private static readonly string[] Keys = { "coach.shift", "coach.contracts", "coach.bestiary", "coach.shop" };

        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly FeatureUnlocks _features;

        public MenuCoachMarks(PlayerProgress progress, PlayerProgressRepository repository, FeatureUnlocks features)
        {
            _progress = progress;
            _repository = repository;
            _features = features;
        }

        public bool IsMarked(CoachMark mark)
        {
            return _features.IsRuleMet(FeatureOf(mark)) && !_progress.HasSeenTip(Keys[(int)mark]);
        }

        public void Dismiss(CoachMark mark)
        {
            if (!IsMarked(mark))
                return;

            _progress.MarkTipSeen(Keys[(int)mark]);
            _repository.Save(_progress);
        }

        private static Feature FeatureOf(CoachMark mark)
        {
            switch (mark)
            {
                case CoachMark.Shift:
                    return Feature.Shift;
                case CoachMark.Bestiary:
                    return Feature.Bestiary;
                case CoachMark.Shop:
                    return Feature.Shop;
                default:
                    return Feature.Contracts;
            }
        }
    }
}
