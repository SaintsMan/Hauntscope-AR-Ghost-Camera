using System;

namespace Hauntscope.Gameplay.Hunt
{
    [Flags]
    public enum PauseReason
    {
        None = 0,
        Manual = 1,
        Background = 2,
        TrackingLost = 4,
        Emergency = 8
    }
}
