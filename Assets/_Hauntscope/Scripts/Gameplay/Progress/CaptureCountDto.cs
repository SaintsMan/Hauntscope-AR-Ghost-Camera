using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Progress
{
    [Serializable]
    public sealed class CaptureCountDto
    {
        [SerializeField] private string _ghostId;
        [SerializeField] private int _count;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public CaptureCountDto()
        {
        }

        public CaptureCountDto(string ghostId, int count)
        {
            _ghostId = ghostId;
            _count = count;
        }

        public string GhostId => _ghostId;

        public int Count => _count;
    }
}
