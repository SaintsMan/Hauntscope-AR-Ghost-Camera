namespace Hauntscope.Gameplay.Tools
{
    // One source the radar hears: which way it lies and how close it is, for the radar screen's blips.
    public readonly struct EmfSignal
    {
        public EmfSignal(float bearing, float proximity)
        {
            Bearing = bearing;
            Proximity = proximity;
        }

        // Degrees from where the camera looks; positive is to the right.
        public float Bearing { get; }

        // 1 right on top of the camera, 0 at the edge of the range.
        public float Proximity { get; }
    }
}
