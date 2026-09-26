using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using UnityEngine;

namespace Hauntscope.Gameplay.Research
{
    // How much the agency knows about a ghost (GDD 5.24): seen in the lens, caught, or studied enough to declassify.
    public sealed class GhostResearch
    {
        private readonly PlayerProgress _progress;
        private readonly ResearchConfig _config;

        public GhostResearch(PlayerProgress progress, ResearchConfig config)
        {
            _progress = progress;
            _config = config;
        }

        public int CapturesToDeclassify => _config.CapturesToDeclassify;

        public float DeclassifiedBonus => _config.DeclassifiedBonus;

        public ResearchLevel GetLevel(GhostData ghost)
        {
            var captures = _progress.GetCaptureCount(ghost.Id);
            if (captures >= _config.CapturesToDeclassify)
                return ResearchLevel.Declassified;
            if (captures > 0)
                return ResearchLevel.Captured;
            return _progress.IsSighted(ghost.Id) ? ResearchLevel.Sighted : ResearchLevel.Unknown;
        }

        public int CapturesLeft(GhostData ghost)
        {
            return Mathf.Max(0, _config.CapturesToDeclassify - _progress.GetCaptureCount(ghost.Id));
        }

        public float RewardMultiplier(GhostData ghost)
        {
            return GetLevel(ghost) == ResearchLevel.Declassified ? 1f + _config.DeclassifiedBonus : 1f;
        }

        // Asked before a catch is counted: will this one declassify the file?
        public bool IsDeclassifyingCatch(GhostData ghost)
        {
            return _progress.GetCaptureCount(ghost.Id) + 1 == _config.CapturesToDeclassify;
        }
    }
}
