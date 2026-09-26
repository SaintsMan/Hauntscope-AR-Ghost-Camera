using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostCapture
    {
        [SerializeField, Min(0.01f)] private float _resistance = 1f;
        [SerializeField, Min(0)] private int _reward = 10;
        [SerializeField, Min(0f)] private float _surgeJerk = 0.3f;

        public GhostCapture()
        {
        }

        public GhostCapture(float resistance, int reward, float surgeJerk = 0.3f)
        {
            _resistance = resistance;
            _reward = reward;
            _surgeJerk = surgeJerk;
        }

        public float Resistance => _resistance;

        public int Reward => _reward;

        // How far the ghost thrashes in its last surge, in metres: gentle for the tutorial wisp, wild for the mimic.
        public float SurgeJerk => _surgeJerk;
    }
}
