using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeCameraPermission : ICameraPermission
    {
        private UniTaskCompletionSource<PermissionResult> _pending;

        public bool IsGranted { get; set; }

        public PermissionResult NextResult { get; set; } = PermissionResult.Granted;

        public bool HoldRequests { get; set; }

        public int RequestCount { get; private set; }

        public int OpenSettingsCount { get; private set; }

        public UniTask<PermissionResult> RequestAsync(CancellationToken cancellationToken)
        {
            RequestCount++;
            if (!HoldRequests)
                return UniTask.FromResult(Answer(NextResult));

            _pending = new UniTaskCompletionSource<PermissionResult>();
            return _pending.Task;
        }

        public void CompletePending(PermissionResult result)
        {
            _pending.TrySetResult(Answer(result));
        }

        public void OpenAppSettings()
        {
            OpenSettingsCount++;
        }

        private PermissionResult Answer(PermissionResult result)
        {
            IsGranted = result == PermissionResult.Granted;
            return result;
        }
    }
}
