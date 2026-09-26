using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class GhostMotion
    {
        [SerializeField, Min(0f)] private float _moveSpeed = 0.5f;
        [SerializeField, Min(0f)] private float _fleeSpeed = 1.2f;
        [SerializeField, Min(0f)] private float _hoverHeightMin = 0.8f;
        [SerializeField, Min(0f)] private float _hoverHeightMax = 1.8f;
        [SerializeField] private float _bodyBottom = -0.5f;
        [SerializeField] private float _bodyTop = 0.5f;

        public GhostMotion()
        {
        }

        public GhostMotion(float moveSpeed, float fleeSpeed, float hoverHeightMin, float hoverHeightMax,
            float bodyBottom = -0.5f, float bodyTop = 0.5f)
        {
            _bodyBottom = bodyBottom;
            _bodyTop = bodyTop;
            _moveSpeed = moveSpeed;
            _fleeSpeed = fleeSpeed;
            _hoverHeightMin = hoverHeightMin;
            _hoverHeightMax = hoverHeightMax;
        }

        public float MoveSpeed => _moveSpeed;

        public float FleeSpeed => _fleeSpeed;

        public float HoverHeightMin => _hoverHeightMin;

        public float HoverHeightMax => _hoverHeightMax;

        // Where the ghost's body starts and ends above and below its pivot, in metres, so a photo can tell whether
        // the whole ghost is in frame. Taken from the generated mesh.
        public float BodyBottom => _bodyBottom;

        public float BodyTop => _bodyTop;
    }
}
