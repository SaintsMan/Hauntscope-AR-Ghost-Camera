using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.31: field tips, each shown once, when its situation first comes up.
    [Serializable]
    public sealed class TipsConfig
    {
        [SerializeField, Min(0.5f)] private float _duration = 5f;
        [SerializeField, Min(0f)] private float _closeInDistance = 2f;
        [SerializeField, Min(0f)] private float _closeInHold = 2f;
        [SerializeField, Min(0)] private int _coldSpotEmfLevel = 3;
        [SerializeField, Min(0)] private int _photoFromHunt = 2;
        [SerializeField] private string _catGhostId = "phantom_cat";
        [SerializeField, Min(0)] private int _contractsMarkAfter = 1;

        public TipsConfig()
        {
        }

        public TipsConfig(float duration, float closeInDistance, float closeInHold, int coldSpotEmfLevel, int photoFromHunt, string catGhostId)
        {
            _duration = duration;
            _closeInDistance = closeInDistance;
            _closeInHold = closeInHold;
            _coldSpotEmfLevel = coldSpotEmfLevel;
            _photoFromHunt = photoFromHunt;
            _catGhostId = catGhostId;
        }

        // How long a tip stays up if the player does not act on it.
        public float Duration => _duration;

        // The beam holding a ghost farther than this, for this long, suggests stepping in.
        public float CloseInDistance => _closeInDistance;

        public float CloseInHold => _closeInHold;

        public int ColdSpotEmfLevel => _coldSpotEmfLevel;

        // The photo tip waits for this hunt (1-based), so the first hunt teaches only the tools.
        public int PhotoFromHunt => _photoFromHunt;

        public string CatGhostId => _catGhostId;

        // Hunts played before CONTRACTS gets its NEW mark.
        public int ContractsMarkAfter => _contractsMarkAfter;
    }
}
