using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // One mark of ectoplasm on the floor.
    public readonly struct TrailMark
    {
        public TrailMark(Vector3 position, TrailMarkKind kind, float time, float yaw)
        {
            Position = position;
            Kind = kind;
            Time = time;
            Yaw = yaw;
        }

        public Vector3 Position { get; }

        public TrailMarkKind Kind { get; }

        // Hunt time when it was left; a mark with no time yet (default) is an empty slot.
        public float Time { get; }

        // Which way the ghost was heading, so drips smear along the path.
        public float Yaw { get; }

        public bool IsEmpty => Time <= 0f;
    }
}
