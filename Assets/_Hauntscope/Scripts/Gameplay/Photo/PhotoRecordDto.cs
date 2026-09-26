using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Photo
{
    [Serializable]
    public sealed class PhotoRecordDto
    {
        [SerializeField] private string _fileName;
        [SerializeField] private string _ghostId;
        [SerializeField] private int _stars;
        [SerializeField] private long _takenUnixSeconds;

        public PhotoRecordDto()
        {
        }

        public PhotoRecordDto(string fileName, string ghostId, int stars, long takenUnixSeconds)
        {
            _fileName = fileName;
            _ghostId = ghostId;
            _stars = stars;
            _takenUnixSeconds = takenUnixSeconds;
        }

        public string FileName => _fileName;

        public string GhostId => _ghostId;

        public int Stars => _stars;

        public long TakenUnixSeconds => _takenUnixSeconds;
    }
}
