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
        // Added without a version bump: saves written before these fields simply read the default.
        [SerializeField] private bool _tutorialCompleted;
        [SerializeField] private int _reviewPromptedAtCaptures;
        [SerializeField] private List<string> _sighted = new List<string>();
        [SerializeField] private int _fieldDropDay;
        [SerializeField] private int _fieldDropsClaimed;
        [SerializeField] private List<CaptureCountDto> _photoEvidence = new List<CaptureCountDto>();
        [SerializeField] private int _shiftsCompleted;
        [SerializeField] private int _bestShiftRound;
        [SerializeField] private List<string> _seenTips = new List<string>();

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
            bool tutorialCompleted,
            int reviewPromptedAtCaptures,
            List<string> sighted,
            int fieldDropDay,
            int fieldDropsClaimed,
            List<CaptureCountDto> photoEvidence,
            int shiftsCompleted,
            int bestShiftRound,
            List<string> seenTips)
        {
            _seenTips = seenTips;
            _shiftsCompleted = shiftsCompleted;
            _bestShiftRound = bestShiftRound;
            _photoEvidence = photoEvidence;
            _sighted = sighted;
            _fieldDropDay = fieldDropDay;
            _fieldDropsClaimed = fieldDropsClaimed;
            _version = version;
            _ectoplasm = ectoplasm;
            _captures = captures;
            _totalSessions = totalSessions;
            _virtualRoomNoticeShown = virtualRoomNoticeShown;
            _tutorialCompleted = tutorialCompleted;
            _reviewPromptedAtCaptures = reviewPromptedAtCaptures;
        }

        public int Version => _version;

        public int Ectoplasm => _ectoplasm;

        public IReadOnlyList<CaptureCountDto> Captures => _captures;

        public int TotalSessions => _totalSessions;

        public bool VirtualRoomNoticeShown => _virtualRoomNoticeShown;

        public bool TutorialCompleted => _tutorialCompleted;

        public int ReviewPromptedAtCaptures => _reviewPromptedAtCaptures;

        public IReadOnlyList<string> Sighted => _sighted;

        public int FieldDropDay => _fieldDropDay;

        public int FieldDropsClaimed => _fieldDropsClaimed;

        public IReadOnlyList<CaptureCountDto> PhotoEvidence => _photoEvidence;

        public int ShiftsCompleted => _shiftsCompleted;

        public int BestShiftRound => _bestShiftRound;

        public IReadOnlyList<string> SeenTips => _seenTips;
    }
}
