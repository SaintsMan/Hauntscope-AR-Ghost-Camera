using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // The last marks a ghost left on the floor, in a fixed ring: the newest overwrites the oldest, nothing is
    // allocated after construction, and each slot keeps its index so a view can pool one decal per slot.
    public sealed class GhostTrail
    {
        private readonly TrailMark[] _marks;
        private readonly float _spacing;

        private int _next;
        private Vector3 _lastDrip;
        private bool _hasDrip;

        public GhostTrail(int capacity, float spacing)
        {
            _marks = new TrailMark[capacity];
            _spacing = spacing;
        }

        public int Capacity => _marks.Length;

        public TrailMark this[int index] => _marks[index];

        // A drip every Spacing metres of path; standing still leaves nothing new.
        public void Follow(Vector3 floorPosition, float time)
        {
            if (_hasDrip)
            {
                var step = floorPosition - _lastDrip;
                step.y = 0f;
                if (step.magnitude < _spacing)
                    return;

                Add(new TrailMark(floorPosition, TrailMarkKind.Drip, time, Mathf.Atan2(step.x, step.z) * Mathf.Rad2Deg));
            }

            _lastDrip = floorPosition;
            _hasDrip = true;
        }

        public void Mark(Vector3 floorPosition, TrailMarkKind kind, float time)
        {
            Add(new TrailMark(floorPosition, kind, time, 0f));
        }

        // A jump breaks the path: the next drip starts from where the ghost landed, not a smear across the room.
        public void Restart(Vector3 floorPosition)
        {
            _lastDrip = floorPosition;
            _hasDrip = true;
        }

        public void Clear()
        {
            for (var i = 0; i < _marks.Length; i++)
                _marks[i] = default;
            _next = 0;
            _hasDrip = false;
        }

        private void Add(TrailMark mark)
        {
            _marks[_next] = mark;
            _next = (_next + 1) % _marks.Length;
        }
    }
}
