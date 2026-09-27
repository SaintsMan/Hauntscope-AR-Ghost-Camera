using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Research;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Story
{
    // Which of Vale's tapes have surfaced and which the agent has heard (GDD 5.35). A tape surfaces the moment its
    // milestone is reached; Unlocked tells the result card. Tapes already earned when the game starts (an update, a
    // restored save) are simply there, new, without a fanfare each.
    public sealed class TapeArchive : IInitializable, IDisposable
    {
        private const string IntroKey = "story.intro";

        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly GhostResearch _research;
        private readonly GhostConfig _ghosts;
        private readonly StoryConfig _config;
        private readonly HashSet<int> _unlocked = new HashSet<int>();

        public TapeArchive(PlayerProgress progress, PlayerProgressRepository repository, GhostResearch research, GhostConfig ghosts,
            StoryConfig config)
        {
            _progress = progress;
            _repository = repository;
            _research = research;
            _ghosts = ghosts;
            _config = config;
        }

        public event Action<StoryTape> Unlocked;

        public IReadOnlyList<StoryTape> Tapes => _config.Tapes;

        public IReadOnlyList<string> IntroPages => _config.IntroPages;

        public bool IsIntroSeen => _progress.HasSeenTip(IntroKey);

        public int UnlockedCount => _unlocked.Count;

        public int UnheardCount
        {
            get
            {
                var count = 0;
                foreach (var tape in _config.Tapes)
                {
                    if (IsUnlocked(tape) && !IsHeard(tape))
                        count++;
                }

                return count;
            }
        }

        public void Initialize()
        {
            Refresh(false);
            _progress.Changed += OnProgressChanged;
        }

        public void Dispose()
        {
            _progress.Changed -= OnProgressChanged;
        }

        public bool IsUnlocked(StoryTape tape)
        {
            return _unlocked.Contains(tape.Number);
        }

        public bool IsHeard(StoryTape tape)
        {
            return _progress.HasSeenTip(tape.HeardKey);
        }

        public void MarkHeard(StoryTape tape)
        {
            if (IsUnlocked(tape) && !IsHeard(tape))
                Remember(tape.HeardKey);
        }

        public void MarkIntroSeen()
        {
            if (!IsIntroSeen)
                Remember(IntroKey);
        }

        private void Remember(string key)
        {
            _progress.MarkTipSeen(key);
            _repository.Save(_progress);
        }

        private void OnProgressChanged()
        {
            Refresh(true);
        }

        private void Refresh(bool announce)
        {
            foreach (var tape in _config.Tapes)
            {
                if (_unlocked.Contains(tape.Number) || Reached(tape.Trigger) < tape.Threshold)
                    continue;

                _unlocked.Add(tape.Number);
                if (announce)
                    Unlocked?.Invoke(tape);
            }
        }

        private int Reached(TapeTrigger trigger)
        {
            switch (trigger)
            {
                case TapeTrigger.DifferentGhosts:
                    return Count(ghost => _progress.GetCaptureCount(ghost.Id) > 0);
                case TapeTrigger.Declassified:
                    return Count(ghost => _research.GetLevel(ghost) == ResearchLevel.Declassified);
                case TapeTrigger.Shifts:
                    return _progress.ShiftsCompleted;
                default:
                    return _progress.TotalCaptures;
            }
        }

        private int Count(Func<GhostData, bool> match)
        {
            var count = 0;
            foreach (var ghost in _ghosts.Ghosts)
            {
                if (match(ghost))
                    count++;
            }

            return count;
        }
    }
}
