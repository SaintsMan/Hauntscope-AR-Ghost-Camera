using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeArAvailability : IArAvailability
    {
        public ArAvailabilityResult Availability { get; set; } = ArAvailabilityResult.Supported;

        public bool InstallSucceeds { get; set; }

        public int CheckCount { get; private set; }

        public int InstallCount { get; private set; }

        public UniTask<ArAvailabilityResult> CheckAsync(CancellationToken cancellationToken)
        {
            CheckCount++;
            return UniTask.FromResult(Availability);
        }

        public UniTask<bool> TryInstallAsync(CancellationToken cancellationToken)
        {
            InstallCount++;
            return UniTask.FromResult(InstallSucceeds);
        }
    }
}
