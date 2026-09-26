using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Shift;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.27: the night shift, three hunts in a row that get harder and pay more.
    [Serializable]
    public sealed class ShiftConfig
    {
        [SerializeField, Min(1)] private int _length = 3;
        [SerializeField, Min(0)] private int _unlockHunts = 3;
        [SerializeField, Range(0f, 1f)] private float _roundRecharge = 0.25f;
        [SerializeField, Min(0f)] private float _resistanceStep = 0.15f;
        [SerializeField, Min(0f)] private float _speedStep = 0.1f;
        [SerializeField] private float[] _rewardMultipliers = { 1f, 1.5f, 2f };
        [SerializeField] private RarityWeights[] _roundWeights =
        {
            new RarityWeights(60f, 30f, 10f),
            new RarityWeights(30f, 50f, 20f),
            new RarityWeights(0f, 60f, 40f)
        };
        [SerializeField, Min(0)] private int _completeBonus = 40;
        [SerializeField, Min(1)] private int _perkOffer = 3;
        [SerializeField] private ShiftPerkData[] _perks = Array.Empty<ShiftPerkData>();

        public ShiftConfig()
        {
        }

        public ShiftConfig(int length, int unlockHunts, float roundRecharge, float resistanceStep, float speedStep,
            float[] rewardMultipliers, RarityWeights[] roundWeights, int completeBonus, int perkOffer, ShiftPerkData[] perks)
        {
            _length = length;
            _unlockHunts = unlockHunts;
            _roundRecharge = roundRecharge;
            _resistanceStep = resistanceStep;
            _speedStep = speedStep;
            _rewardMultipliers = rewardMultipliers;
            _roundWeights = roundWeights;
            _completeBonus = completeBonus;
            _perkOffer = perkOffer;
            _perks = perks;
        }

        public int Length => _length;

        // Hunts played before the shift opens in the menu.
        public int UnlockHunts => _unlockHunts;

        // Added to the battery carried into the next round.
        public float RoundRecharge => _roundRecharge;

        public float ResistanceStep => _resistanceStep;

        public float SpeedStep => _speedStep;

        public int CompleteBonus => _completeBonus;

        public int PerkOffer => _perkOffer;

        public IReadOnlyList<ShiftPerkData> Perks => _perks;

        // Rounds past the end of a list keep its last value.
        public float RewardMultiplier(int round)
        {
            return _rewardMultipliers.Length == 0 ? 1f : _rewardMultipliers[Mathf.Clamp(round, 0, _rewardMultipliers.Length - 1)];
        }

        public RarityWeights Weights(int round)
        {
            return _roundWeights.Length == 0 ? null : _roundWeights[Mathf.Clamp(round, 0, _roundWeights.Length - 1)];
        }
    }
}
