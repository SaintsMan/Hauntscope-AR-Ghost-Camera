using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Progress
{
    // JsonUtility can't serialize dictionaries, so capture counts are stored as a list of entries.
    [Serializable]
    public sealed class PlayerProgressDto
    {
        [SerializeField] private int _version;
        [SerializeField] private int _ectoplasm;
        [SerializeField] private List<CaptureCountDto> _captures = new List<CaptureCountDto>();
        [SerializeField] private int _totalSessions;
        [SerializeField] private bool _virtualRoomNoticeShown;
        // Added without a version bump: saves written before it simply read false.
        [SerializeField] private bool _tutorialCompleted;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public PlayerProgressDto()
        {
        }

        public PlayerProgressDto(
            int version,
            int ectoplasm,
            List<CaptureCountDto> captures,
            int totalSessions,
            bool virtualRoomNoticeShown,
            bool tutorialCompleted)
        {
            _version = version;
            _ectoplasm = ectoplasm;
            _captures = captures;
            _totalSessions = totalSessions;
            _virtualRoomNoticeShown = virtualRoomNoticeShown;
            _tutorialCompleted = tutorialCompleted;
        }

        public int Version => _version;

        public int Ectoplasm => _ectoplasm;

        public IReadOnlyList<CaptureCountDto> Captures => _captures;

        public int TotalSessions => _totalSessions;

        public bool VirtualRoomNoticeShown => _virtualRoomNoticeShown;

        public bool TutorialCompleted => _tutorialCompleted;
    }
}
