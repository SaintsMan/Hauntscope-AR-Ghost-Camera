using System;
using System.Collections.Generic;
using Hauntscope.Core.Observables;

namespace Hauntscope.Gameplay.Progress
{
    public sealed class PlayerProgress
    {
        private readonly ObservableValue<int> _ectoplasm;
        private readonly Dictionary<string, int> _captures;
        private readonly HashSet<string> _sighted = new HashSet<string>();
        private readonly Dictionary<string, int> _photoEvidence = new Dictionary<string, int>();

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
            int reviewPromptedAtCaptures = 0,
            IEnumerable<string> sighted = null,
            int fieldDropDay = 0,
            int fieldDropsClaimed = 0,
            IReadOnlyDictionary<string, int> photoEvidence = null)
        {
            if (photoEvidence != null)
                foreach (var pair in photoEvidence)
                    _photoEvidence[pair.Key] = pair.Value;
            FieldDropDay = fieldDropDay;
            FieldDropsClaimed = fieldDropsClaimed;
            _ectoplasm = new ObservableValue<int>(ectoplasm);
            _captures = new Dictionary<string, int>();
            foreach (var pair in captures)
                _captures[pair.Key] = pair.Value;
            TotalSessions = totalSessions;
            VirtualRoomNoticeShown = virtualRoomNoticeShown;
            TutorialCompleted = tutorialCompleted;
            ReviewPromptedAtCaptures = reviewPromptedAtCaptures;
            if (sighted != null)
                _sighted.UnionWith(sighted);
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

        // Day key (yyyymmdd) of the last field drop claim and how many were claimed that day.
        public int FieldDropDay { get; private set; }

        public int FieldDropsClaimed { get; private set; }

        public int FieldDropsClaimedOn(int day)
        {
            return day == FieldDropDay ? FieldDropsClaimed : 0;
        }

        public void ClaimFieldDrop(int day, int reward)
        {
            if (day != FieldDropDay)
            {
                FieldDropDay = day;
                FieldDropsClaimed = 0;
            }

            FieldDropsClaimed++;
            AddEctoplasm(reward);
        }

        // Ghosts revealed in the lens at least once, caught or not.
        public IReadOnlyCollection<string> Sighted => _sighted;

        public bool IsSighted(string ghostId)
        {
            return _sighted.Contains(ghostId);
        }

        public void MarkSighted(string ghostId)
        {
            if (_sighted.Add(ghostId))
                Changed?.Invoke();
        }

        // Photos good enough to count as research, per ghost; capped so photos alone never declassify a file.
        public IReadOnlyDictionary<string, int> PhotoEvidence => _photoEvidence;

        public int GetPhotoEvidence(string ghostId)
        {
            return _photoEvidence.TryGetValue(ghostId, out var count) ? count : 0;
        }

        public void AddPhotoEvidence(string ghostId, int count, int cap)
        {
            var current = GetPhotoEvidence(ghostId);
            var next = System.Math.Min(cap, current + count);
            if (count <= 0 || next == current)
                return;

            _photoEvidence[ghostId] = next;
            Changed?.Invoke();
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
