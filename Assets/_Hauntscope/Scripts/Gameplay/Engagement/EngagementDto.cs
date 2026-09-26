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
        // Added without a version bump: saves written before these fields simply read the default.
        [SerializeField] private int _loginClaims;
        [SerializeField] private int _lastLoginDay;
        [SerializeField] private int _lastLoginShownDay;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public EngagementDto()
        {
        }

        public EngagementDto(int version, int contractDay, List<ContractSlotDto> contracts, int freeReplacesUsed, int adReplacesUsed,
            int loginClaims, int lastLoginDay, int lastLoginShownDay)
        {
            _loginClaims = loginClaims;
            _lastLoginDay = lastLoginDay;
            _lastLoginShownDay = lastLoginShownDay;
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

        public int LoginClaims => _loginClaims;

        public int LastLoginDay => _lastLoginDay;

        public int LastLoginShownDay => _lastLoginShownDay;
    }
}
