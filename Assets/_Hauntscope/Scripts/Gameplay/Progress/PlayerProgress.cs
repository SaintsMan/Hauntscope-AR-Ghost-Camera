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

        public PlayerProgress(int ectoplasm, IReadOnlyDictionary<string, int> captures, int totalSessions, bool virtualRoomNoticeShown)
        {
            _ectoplasm = new ObservableValue<int>(ectoplasm);
            _captures = new Dictionary<string, int>();
            foreach (var pair in captures)
                _captures[pair.Key] = pair.Value;
            TotalSessions = totalSessions;
            VirtualRoomNoticeShown = virtualRoomNoticeShown;
        }

        public event Action Changed;

        public IReadOnlyObservableValue<int> Ectoplasm => _ectoplasm;

        public IReadOnlyDictionary<string, int> Captures => _captures;

        public int TotalSessions { get; private set; }

        public bool IsFirstSession => TotalSessions == 0;

        public bool VirtualRoomNoticeShown { get; private set; }

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

        public void MarkVirtualRoomNoticeShown()
        {
            VirtualRoomNoticeShown = true;
            Changed?.Invoke();
        }
    }
}
