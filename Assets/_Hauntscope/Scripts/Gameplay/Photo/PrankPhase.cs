namespace Hauntscope.Gameplay.Photo
{
    public enum PrankPhase
    {
        // Off: a hunt, or the room is still being scanned.
        Off,

        // The ghost stands where the camera points at the floor and follows it.
        Aiming,

        // Pinned to the floor: drags move it, two fingers turn and size it.
        Placed
    }
}
