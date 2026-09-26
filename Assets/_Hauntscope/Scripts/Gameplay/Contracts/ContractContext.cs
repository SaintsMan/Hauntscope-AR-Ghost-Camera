using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Contracts
{
    // What a goal may look at when deciding whether it can be offered today: only mechanics the player has opened.
    public sealed class ContractContext
    {
        public ContractContext(PlayerProgress progress, IReadOnlyList<GhostData> ghosts, ShiftConfig shift, IRandom random)
        {
            Progress = progress;
            Ghosts = ghosts;
            Shift = shift;
            Random = random;
        }

        public PlayerProgress Progress { get; }

        public IReadOnlyList<GhostData> Ghosts { get; }

        public ShiftConfig Shift { get; }

        public IRandom Random { get; }

        public bool IsShiftUnlocked => Progress.TotalSessions >= Shift.UnlockHunts;
    }
}
