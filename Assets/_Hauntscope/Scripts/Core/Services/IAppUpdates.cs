using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    public interface IAppUpdates
    {
        // The build number of the installed game (Android versionCode).
        int InstalledVersionCode { get; }

        UniTask<bool> TryStartUpdateAsync(CancellationToken cancellationToken);

        // The game's store page, for when the in-app update flow cannot start (installed outside the store).
        void OpenStorePage();
    }
}
