using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    // The rookie's beam holds a ghost on a leash: wherever it runs or jumps, it stays within this far of the camera
    // on the floor plane, so it never leaves the lens' reveal range while the beam is on it.
    public static class GhostLeash
    {
        public static Vector3 Clamp(Vector3 position, Vector3 camera, float leash)
        {
            if (leash <= 0f)
                return position;

            var offset = position - camera;
            offset.y = 0f;
            if (offset.sqrMagnitude <= leash * leash)
                return position;

            var clamped = camera + offset.normalized * leash;
            clamped.y = position.y;
            return clamped;
        }
    }
}
