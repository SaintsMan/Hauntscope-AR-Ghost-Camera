using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Boot
{
    // Checking ARCore here warms up the availability query HuntLauncher repeats on START HUNT.
    public sealed class SensorsBootTask : IBootTask
    {
        private const string ArReadyKey = "splash.status.ar_ready";
        private const string ArInstallKey = "splash.status.ar_install";
        private const string VirtualKey = "splash.status.virtual";

        private readonly IArAvailability _arAvailability;

        public SensorsBootTask(IArAvailability arAvailability)
        {
            _arAvailability = arAvailability;
        }

        public string LabelKey => "splash.step.sensors";

        public async UniTask<string> RunAsync(CancellationToken cancellationToken)
        {
            var availability = await _arAvailability.CheckAsync(cancellationToken);
            switch (availability)
            {
                case ArAvailabilityResult.Supported:
                    return ArReadyKey;
                case ArAvailabilityResult.NeedsInstall:
                    return ArInstallKey;
                default:
                    return VirtualKey;
            }
        }
    }
}
