using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Gameplay.Environment
{
    public interface IArAvailability
    {
        UniTask<ArAvailabilityResult> CheckAsync(CancellationToken cancellationToken);

        UniTask<bool> TryInstallAsync(CancellationToken cancellationToken);
    }
}
