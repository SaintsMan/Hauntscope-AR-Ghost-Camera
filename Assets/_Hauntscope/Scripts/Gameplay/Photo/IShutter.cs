using System;

namespace Hauntscope.Gameplay.Photo
{
    // Anything that takes a still: the spirit camera in a hunt and the prank camera.
    public interface IShutter
    {
        event Action ShutterReleased;
    }
}
