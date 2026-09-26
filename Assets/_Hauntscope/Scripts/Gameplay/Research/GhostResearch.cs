using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using UnityEngine;

namespace Hauntscope.Gameplay.Research
{
    // How much the agency knows about a ghost (GDD 5.24): seen in the lens, caught, or studied enough to declassify.
    // Evidence = captures plus a few good photos; a file still needs at least one capture to open at all.
    public sealed class GhostResearch
    {
        private readonly PlayerProgress _progress;
        private readonly ResearchConfig _config;

        public GhostResearch(PlayerProgress progress, ResearchConfig config)
        {
            _progress = progress;
            _config = config;
        }

        public int EvidenceToDeclassify => _config.CapturesToDeclassify;

        public float DeclassifiedBonus => _config.DeclassifiedBonus;

        public int MaxPhotoEvidence => _config.MaxPhotoEvidence;

        public ResearchLevel GetLevel(GhostData ghost)
        {
            var captures = _progress.GetCaptureCount(ghost.Id);
            if (captures > 0 && Evidence(ghost) >= _config.CapturesToDeclassify)
                return ResearchLevel.Declassified;
            if (captures > 0)
                return ResearchLevel.Captured;
            return _progress.IsSighted(ghost.Id) ? ResearchLevel.Sighted : ResearchLevel.Unknown;
        }

        public int EvidenceLeft(GhostData ghost)
        {
            return Mathf.Max(0, _config.CapturesToDeclassify - Evidence(ghost));
        }

        // Only the first few good photos count: past the cap, the file needs captures.
        public int PhotoEvidence(GhostData ghost)
        {
            return Mathf.Min(_config.MaxPhotoEvidence, _progress.GetPhotoEvidence(ghost.Id));
        }

        public float RewardMultiplier(GhostData ghost)
        {
            return GetLevel(ghost) == ResearchLevel.Declassified ? 1f + _config.DeclassifiedBonus : 1f;
        }

        // Asked before a hunt's results are banked: will this catch and these photos declassify the file?
        public bool WillDeclassify(GhostData ghost, bool captured, int photoEvidence)
        {
            if (GetLevel(ghost) == ResearchLevel.Declassified)
                return false;

            var captures = _progress.GetCaptureCount(ghost.Id) + (captured ? 1 : 0);
            var photos = Mathf.Min(_config.MaxPhotoEvidence, _progress.GetPhotoEvidence(ghost.Id) + photoEvidence);
            return captures > 0 && captures + photos >= _config.CapturesToDeclassify;
        }

        private int Evidence(GhostData ghost)
        {
            return _progress.GetCaptureCount(ghost.Id) + PhotoEvidence(ghost);
        }
    }
}
