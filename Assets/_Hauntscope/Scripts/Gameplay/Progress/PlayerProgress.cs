using System;
using System.Collections.Generic;
using Hauntscope.Core.Observables;

namespace Hauntscope.Gameplay.Progress
{
    public sealed class PlayerProgress
    {
        private readonly ObservableValue<int> _ectoplasm;
        private readonly Dictionary<string, int> _captures;

        public PlayerProgress()
            : this(0, new Dictionary<string, int>(), 0, false)
        {
        }

        public PlayerProgress(
            int ectoplasm,
            IReadOnlyDictionary<string, int> captures,
            int totalSessions,
            bool virtualRoomNoticeShown,
            bool tutorialCompleted = false,
            int reviewPromptedAtCaptures = 0)
        {
            _ectoplasm = new ObservableValue<int>(ectoplasm);
            _captures = new Dictionary<string, int>();
            foreach (var pair in captures)
                _captures[pair.Key] = pair.Value;
            TotalSessions = totalSessions;
            VirtualRoomNoticeShown = virtualRoomNoticeShown;
            TutorialCompleted = tutorialCompleted;
            ReviewPromptedAtCaptures = reviewPromptedAtCaptures;
        }

        public event Action Changed;

        public IReadOnlyObservableValue<int> Ectoplasm => _ectoplasm;

        public IReadOnlyDictionary<string, int> Captures => _captures;

        public int TotalSessions { get; private set; }

        public bool IsFirstSession => TotalSessions == 0;

        public bool VirtualRoomNoticeShown { get; private set; }

        public bool TutorialCompleted { get; private set; }

        // Total captures at the moment the store review was last requested; 0 means never.
        public int ReviewPromptedAtCaptures { get; private set; }

        public int TotalCaptures
        {
            get
            {
                var total = 0;
                foreach (var count in _captures.Values)
                    total += count;
                return total;
            }
        }

        public int GetCaptureCount(string ghostId)
        {
            return _captures.TryGetValue(ghostId, out var count) ? count : 0;
        }

        public void RegisterSession()
        {
            TotalSessions++;
            Changed?.Invoke();
        }

        public void AddCapture(string ghostId, int reward)
        {
            _captures[ghostId] = GetCaptureCount(ghostId) + 1;
            _ectoplasm.Value += reward;
            Changed?.Invoke();
        }

        public void AddEctoplasm(int amount)
        {
            if (amount <= 0)
                return;

            _ectoplasm.Value += amount;
            Changed?.Invoke();
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || _ectoplasm.Value < amount)
                return false;

            _ectoplasm.Value -= amount;
            Changed?.Invoke();
            return true;
        }

        public void MarkTutorialCompleted()
        {
            TutorialCompleted = true;
            Changed?.Invoke();
        }

        public void MarkReviewPrompted()
        {
            ReviewPromptedAtCaptures = TotalCaptures;
            Changed?.Invoke();
        }

        public void MarkVirtualRoomNoticeShown()
        {
            VirtualRoomNoticeShown = true;
            Changed?.Invoke();
        }
    }
}
