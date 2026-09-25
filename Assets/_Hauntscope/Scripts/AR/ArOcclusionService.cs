using Hauntscope.Gameplay.Environment;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Hauntscope.AR
{
    public sealed class ArOcclusionService : IOcclusionService
    {
        private readonly AROcclusionManager _manager;

        public ArOcclusionService(AROcclusionManager manager)
        {
            _manager = manager;
        }

        // ARCore answers "Unknown" until the session is running, so support is asked for on demand, not cached.
        public bool IsSupported => _manager.descriptor != null
            && _manager.descriptor.environmentDepthImageSupported == Supported.Supported;

        public void SetEnabled(bool enabled)
        {
            // With environment depth the camera background writes real-world depth, so ghosts hide behind furniture.
            _manager.requestedEnvironmentDepthMode = enabled ? EnvironmentDepthMode.Medium : EnvironmentDepthMode.Disabled;
            _manager.requestedOcclusionPreferenceMode = OcclusionPreferenceMode.PreferEnvironmentOcclusion;
            _manager.environmentDepthTemporalSmoothingRequested = true;
        }
    }
}
