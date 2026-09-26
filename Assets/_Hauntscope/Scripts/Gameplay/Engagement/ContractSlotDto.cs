using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Engagement
{
    [Serializable]
    public sealed class ContractSlotDto
    {
        [SerializeField] private string _contractId;
        [SerializeField] private string _subject;
        [SerializeField] private int _progress;
        [SerializeField] private bool _claimed;
        [SerializeField] private int _ectoplasm;
        [SerializeField] private string _gearId;
        [SerializeField] private int _gearCount;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public ContractSlotDto()
        {
        }

        public ContractSlotDto(string contractId, string subject, int progress, bool claimed, int ectoplasm, string gearId, int gearCount)
        {
            _contractId = contractId;
            _subject = subject;
            _progress = progress;
            _claimed = claimed;
            _ectoplasm = ectoplasm;
            _gearId = gearId;
            _gearCount = gearCount;
        }

        public string ContractId => _contractId;

        public string Subject => _subject;

        public int Progress => _progress;

        public bool Claimed => _claimed;

        public int Ectoplasm => _ectoplasm;

        public string GearId => _gearId;

        public int GearCount => _gearCount;
    }
}
