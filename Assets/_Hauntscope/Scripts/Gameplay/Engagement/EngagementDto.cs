using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Engagement
{
    [Serializable]
    public sealed class EngagementDto
    {
        [SerializeField] private int _version;
        [SerializeField] private int _contractDay;
        [SerializeField] private List<ContractSlotDto> _contracts = new List<ContractSlotDto>();
        [SerializeField] private int _freeReplacesUsed;
        [SerializeField] private int _adReplacesUsed;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public EngagementDto()
        {
        }

        public EngagementDto(int version, int contractDay, List<ContractSlotDto> contracts, int freeReplacesUsed, int adReplacesUsed)
        {
            _version = version;
            _contractDay = contractDay;
            _contracts = contracts;
            _freeReplacesUsed = freeReplacesUsed;
            _adReplacesUsed = adReplacesUsed;
        }

        public int Version => _version;

        public int ContractDay => _contractDay;

        public IReadOnlyList<ContractSlotDto> Contracts => _contracts;

        public int FreeReplacesUsed => _freeReplacesUsed;

        public int AdReplacesUsed => _adReplacesUsed;
    }
}
