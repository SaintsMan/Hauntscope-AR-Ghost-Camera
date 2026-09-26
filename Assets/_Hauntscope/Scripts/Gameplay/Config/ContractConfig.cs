using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Contracts;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.29: the agency's daily contracts.
    [Serializable]
    public sealed class ContractConfig
    {
        [SerializeField] private ContractTier[] _slots = { ContractTier.Easy, ContractTier.Medium, ContractTier.Hard };
        [SerializeField] private int[] _rewards = { 15, 30, 50 };
        [SerializeField] private float[] _gearChances = { 0f, 0f, 0.3f };
        [SerializeField, Min(0)] private int _freeReplaces = 1;
        [SerializeField, Min(0)] private int _adReplaces = 1;
        [SerializeField, Min(0f)] private float _staggerHitSeconds = 0.5f;
        [SerializeField] private ContractData[] _contracts = Array.Empty<ContractData>();

        public ContractConfig()
        {
        }

        public ContractConfig(ContractTier[] slots, int[] rewards, float[] gearChances, int freeReplaces, int adReplaces,
            float staggerHitSeconds, ContractData[] contracts)
        {
            _slots = slots;
            _rewards = rewards;
            _gearChances = gearChances;
            _freeReplaces = freeReplaces;
            _adReplaces = adReplaces;
            _staggerHitSeconds = staggerHitSeconds;
            _contracts = contracts;
        }

        // The tier of each card on the board, top to bottom.
        public IReadOnlyList<ContractTier> Slots => _slots;

        public int FreeReplaces => _freeReplaces;

        public int AdReplaces => _adReplaces;

        // How long the beam must hold a vulnerable ghost for one hit.
        public float StaggerHitSeconds => _staggerHitSeconds;

        public IReadOnlyList<ContractData> Contracts => _contracts;

        public int Reward(ContractTier tier)
        {
            return ByTier(_rewards, tier, 0);
        }

        // Chance that a contract of the tier pays a booster instead of ectoplasm.
        public float GearChance(ContractTier tier)
        {
            return ByTier(_gearChances, tier, 0f);
        }

        public ContractData Find(string id)
        {
            foreach (var contract in _contracts)
            {
                if (contract != null && contract.Id == id)
                    return contract;
            }

            return null;
        }

        private static T ByTier<T>(T[] values, ContractTier tier, T fallback)
        {
            var index = (int)tier;
            return index >= 0 && index < values.Length ? values[index] : fallback;
        }
    }
}
