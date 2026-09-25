using Hauntscope.Gameplay.Environment;

namespace Hauntscope.VirtualRoom
{
    // Furniture in the Virtual Room is real geometry and already hides ghosts through the depth buffer.
    public sealed class NullOcclusionService : IOcclusionService
    {
        public bool IsSupported => false;

        public void SetEnabled(bool enabled)
        {
        }
    }
}
