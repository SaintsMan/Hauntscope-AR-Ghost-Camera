using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // How far a ghost can go from where it is before the room ends: dashes and yanks aim for the side with space
    // rather than into a wall.
    public static class RoomReach
    {
        public static float Distance(GhostContext context, Vector3 direction)
        {
            var bounds = context.Planes.RoomBounds;
            var position = context.Mover.Position;
            var room = float.MaxValue;
            if (direction.x > Mathf.Epsilon)
                room = Mathf.Min(room, (bounds.max.x - position.x) / direction.x);
            else if (direction.x < -Mathf.Epsilon)
                room = Mathf.Min(room, (bounds.min.x - position.x) / direction.x);
            if (direction.z > Mathf.Epsilon)
                room = Mathf.Min(room, (bounds.max.z - position.z) / direction.z);
            else if (direction.z < -Mathf.Epsilon)
                room = Mathf.Min(room, (bounds.min.z - position.z) / direction.z);

            return Mathf.Max(0f, room);
        }

        // Sideways across the camera's line of sight, to a random side unless that one runs into a wall.
        public static Vector3 Sideways(GhostContext context, float minRoom)
        {
            var lineOfSight = context.Mover.Position - context.Camera.Position;
            lineOfSight.y = 0f;
            if (lineOfSight.sqrMagnitude <= Mathf.Epsilon)
                lineOfSight = new Vector3(context.Camera.Forward.x, 0f, context.Camera.Forward.z);

            var side = new Vector3(lineOfSight.z, 0f, -lineOfSight.x).normalized;
            if (context.Random.Value < 0.5f)
                side = -side;
            if (Distance(context, side) < minRoom)
                side = -side;
            return side;
        }
    }
}
