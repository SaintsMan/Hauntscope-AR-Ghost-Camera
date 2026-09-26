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

            return new PlayerProgress(dto.Ectoplasm, ToCounts(dto.Captures), dto.TotalSessions, dto.VirtualRoomNoticeShown,
                dto.TutorialCompleted, dto.ReviewPromptedAtCaptures, dto.Sighted, dto.FieldDropDay, dto.FieldDropsClaimed,
                ToCounts(dto.PhotoEvidence));
        }

        public void Save(PlayerProgress progress)
        {
            _save.Save(Key, new PlayerProgressDto(CurrentVersion, progress.Ectoplasm.Value, ToDtos(progress.Captures),
                progress.TotalSessions, progress.VirtualRoomNoticeShown, progress.TutorialCompleted,
                progress.ReviewPromptedAtCaptures, new List<string>(progress.Sighted), progress.FieldDropDay, progress.FieldDropsClaimed,
                ToDtos(progress.PhotoEvidence)));
        }

        private static Dictionary<string, int> ToCounts(IReadOnlyList<CaptureCountDto> entries)
        {
            var counts = new Dictionary<string, int>();
            if (entries == null)
                return counts;

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.GhostId) && entry.Count > 0)
                    counts[entry.GhostId] = entry.Count;
            }

            return counts;
        }

        private static List<CaptureCountDto> ToDtos(IReadOnlyDictionary<string, int> counts)
        {
            var entries = new List<CaptureCountDto>(counts.Count);
            foreach (var pair in counts)
                entries.Add(new CaptureCountDto(pair.Key, pair.Value));
            return entries;
        }

        // No migrations exist yet: version 1 is the only format. Unknown (newer or missing) versions start fresh.
        private static bool IsSupported(int version)
        {
            return version >= 1 && version <= CurrentVersion;
        }
    }
}
