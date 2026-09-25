using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostMotion
    {
        [SerializeField, Min(0f)] private float _moveSpeed = 0.5f;
        [SerializeField, Min(0f)] private float _hoverHeightMin = 0.8f;
        [SerializeField, Min(0f)] private float _hoverHeightMax = 1.8f;

        public GhostMotion()
        {
        }

        public GhostMotion(float moveSpeed, float hoverHeightMin, float hoverHeightMax)
        {
            _moveSpeed = moveSpeed;
            _hoverHeightMin = hoverHeightMin;
            _hoverHeightMax = hoverHeightMax;
        }

        public float MoveSpeed => _moveSpeed;

        public float HoverHeightMin => _hoverHeightMin;

        public float HoverHeightMax => _hoverHeightMax;
    }
}
