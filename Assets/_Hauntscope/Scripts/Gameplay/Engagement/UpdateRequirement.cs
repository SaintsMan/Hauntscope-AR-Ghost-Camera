using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Engagement
{
    // Whether this build is too old for what the owner runs now (min_version_code in Remote Config): a broken save
    // format, a store change or an event that needs new code. 0 asks nothing of anyone.
    public sealed class UpdateRequirement
    {
        private readonly IAppUpdates _updates;
        private readonly RemoteTuning _tuning;
        private readonly LiveOpsConfig _config;

        public UpdateRequirement(IAppUpdates updates, RemoteTuning tuning, LiveOpsConfig config)
        {
            _updates = updates;
            _tuning = tuning;
            _config = config;
        }

        public bool IsRequired =>
            _tuning.Int(RemoteKeys.MinVersionCode, _config.MinVersionCode, 0, RemoteKeys.MaxVersionCode) > _updates.InstalledVersionCode;
    }
}
