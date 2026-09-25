using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Gameplay.Environment
{
    public interface ICameraPermission
    {
        bool IsGranted { get; }

        UniTask<PermissionResult> RequestAsync(CancellationToken cancellationToken);

        void OpenAppSettings();
    }
}
