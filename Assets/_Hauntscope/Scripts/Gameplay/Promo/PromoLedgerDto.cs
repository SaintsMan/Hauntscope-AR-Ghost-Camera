using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Promo
{
    [Serializable]
    public sealed class PromoLedgerDto
    {
        [SerializeField] private int _version;
        [SerializeField] private List<string> _codes = new List<string>();

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public PromoLedgerDto()
        {
        }

        public PromoLedgerDto(int version, List<string> codes)
        {
            _version = version;
            _codes = codes;
        }

        public int Version => _version;

        public List<string> Codes => _codes;
    }
}
