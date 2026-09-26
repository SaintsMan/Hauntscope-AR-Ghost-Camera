using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.28: when a ghost that has been seen slips into a hiding spot, how long it stays there and how close the
    // lens has to come to find it.
    [Serializable]
    public sealed class HideConfig
    {
        [SerializeField, Range(0f, 1f)] private float _chance = 0.3f;
        [SerializeField, Min(0f)] private float _cooldown = 20f;
        [SerializeField, Min(0f)] private float _durationMin = 6f;
        [SerializeField, Min(0f)] private float _durationMax = 10f;
        [SerializeField, Min(0f)] private float _floorClearanceMin = 0.02f;
        [SerializeField, Min(0f)] private float _floorClearanceMax = 0.15f;
        [SerializeField, Min(0.1f)] private float _revealRange = 1.2f;
        [SerializeField, Min(0f)] private float _flushStagger = 1f;
        [SerializeField, Min(0f)] private float _minDistance = 2f;
        [SerializeField, Min(0f)] private float _wallOffset = 0.35f;
        [SerializeField, Min(0f)] private float _cornerInset = 0.4f;

        public HideConfig()
        {
        }

        public HideConfig(float chance, float cooldown, float durationMin, float durationMax, float revealRange,
            float flushStagger, float minDistance)
        {
            _chance = chance;
            _cooldown = cooldown;
            _durationMin = durationMin;
            _durationMax = durationMax;
            _revealRange = revealRange;
            _flushStagger = flushStagger;
            _minDistance = minDistance;
        }

        // Rolled every time a ghost that has already been spotted picks a new place to wander to.
        public float Chance => _chance;

        // Counted from the end of the last hide, so a found ghost gets back into the open before it hides again.
        public float Cooldown => _cooldown;

        public float DurationMin => _durationMin;

        public float DurationMax => _durationMax;

        // How far the lowest point of the body floats above the floor: hiding ghosts crouch as low as they can.
        public float FloorClearanceMin => _floorClearanceMin;

        public float FloorClearanceMax => _floorClearanceMax;

        public float RevealRange => _revealRange;

        // Pulled out of hiding by the lens, the ghost is winded for this long before it bolts.
        public float FlushStagger => _flushStagger;

        // Spots closer to the player than this are skipped: the lens should not find a hiding ghost without walking.
        public float MinDistance => _minDistance;

        // AR: how far into the room from a detected wall the spot sits, and how far corners are pulled in.
        public float WallOffset => _wallOffset;

        public float CornerInset => _cornerInset;
    }
}
