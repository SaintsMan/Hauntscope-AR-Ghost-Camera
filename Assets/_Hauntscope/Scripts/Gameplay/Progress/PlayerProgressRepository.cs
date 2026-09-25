using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Progress
{
    public sealed class PlayerProgressRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "player_progress";

        private readonly ISaveService _save;

        public PlayerProgressRepository(ISaveService save)
        {
            _save = save;
        }

        public PlayerProgress Load()
        {
            if (!_save.TryLoad<PlayerProgressDto>(Key, out var dto) || !IsSupported(dto.Version))
                return new PlayerProgress();

            var captures = new Dictionary<string, int>();
            foreach (var entry in dto.Captures)
            {
                if (!string.IsNullOrEmpty(entry.GhostId) && entry.Count > 0)
                    captures[entry.GhostId] = entry.Count;
            }

            return new PlayerProgress(dto.Ectoplasm, captures, dto.TotalSessions, dto.VirtualRoomNoticeShown,
                dto.TutorialCompleted, dto.ReviewPromptedAtCaptures);
        }

        public void Save(PlayerProgress progress)
        {
            var captures = new List<CaptureCountDto>(progress.Captures.Count);
            foreach (var pair in progress.Captures)
                captures.Add(new CaptureCountDto(pair.Key, pair.Value));

            _save.Save(Key, new PlayerProgressDto(CurrentVersion, progress.Ectoplasm.Value, captures,
                progress.TotalSessions, progress.VirtualRoomNoticeShown, progress.TutorialCompleted,
                progress.ReviewPromptedAtCaptures));
        }

        // No migrations exist yet: version 1 is the only format. Unknown (newer or missing) versions start fresh.
        private static bool IsSupported(int version)
        {
            return version >= 1 && version <= CurrentVersion;
        }
    }
}
