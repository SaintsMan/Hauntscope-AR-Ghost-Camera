using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Engagement
{
    // One-off NEW marks in the menu (GDD 5.31): on the night shift once it opens, on CONTRACTS after the first hunt, on the
    // BESTIARY once there is a file in it, on the SHOP once a few hunts have paid for something. A mark goes for good the
    // first time its button is pressed; "Reset tips" brings it back.
    public sealed class MenuCoachMarks
    {
        private static readonly string[] Keys = { "coach.shift", "coach.contracts", "coach.bestiary", "coach.shop" };

        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly ShiftConfig _shift;
        private readonly TipsConfig _tips;

        public MenuCoachMarks(PlayerProgress progress, PlayerProgressRepository repository, ShiftConfig shift, TipsConfig tips)
        {
            _progress = progress;
            _repository = repository;
            _shift = shift;
            _tips = tips;
        }

        public bool IsMarked(CoachMark mark)
        {
            return IsDue(mark) && !_progress.HasSeenTip(Keys[(int)mark]);
        }

        public void Dismiss(CoachMark mark)
        {
            if (!IsMarked(mark))
                return;

            _progress.MarkTipSeen(Keys[(int)mark]);
            _repository.Save(_progress);
        }

        private bool IsDue(CoachMark mark)
        {
            switch (mark)
            {
                case CoachMark.Shift:
                    return _progress.TotalSessions >= _shift.UnlockHunts;
                case CoachMark.Bestiary:
                    return _progress.TotalCaptures >= _tips.BestiaryMarkAfterCaptures;
                case CoachMark.Shop:
                    return _progress.TotalSessions >= _tips.ShopMarkAfter;
                default:
                    return _progress.TotalSessions >= _tips.ContractsMarkAfter;
            }
        }
    }
}
