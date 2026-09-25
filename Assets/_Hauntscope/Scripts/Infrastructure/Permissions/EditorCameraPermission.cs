using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Infrastructure.Permissions
{
    // The Editor has no permission dialog: GameConfig decides what the "player" answers, so denial flows are testable.
    public sealed class EditorCameraPermission : ICameraPermission
    {
        private readonly LaunchConfig _config;

        public EditorCameraPermission(LaunchConfig config)
        {
            _config = config;
        }

        public bool IsGranted => _config.EditorCameraPermission == PermissionResult.Granted;

        public UniTask<PermissionResult> RequestAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(_config.EditorCameraPermission);
        }

        public void OpenAppSettings()
        {
        }
    }
}
