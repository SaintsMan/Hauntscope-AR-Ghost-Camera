using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // How the ghost pool is split between rarities; each share is divided among the ghosts of that rarity.
    [Serializable]
    public sealed class RarityWeights
    {
        [SerializeField, Min(0f)] private float _common;
        [SerializeField, Min(0f)] private float _rare;
        [SerializeField, Min(0f)] private float _legendary;

        public RarityWeights()
        {
        }

        public RarityWeights(float common, float rare, float legendary)
        {
            _common = common;
            _rare = rare;
            _legendary = legendary;
        }

        public float WeightOf(GhostRarity rarity)
        {
            return rarity switch
            {
                GhostRarity.Common => _common,
                GhostRarity.Rare => _rare,
                _ => _legendary
            };
        }
    }
}
