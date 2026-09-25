using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntLauncher : IInitializable, IDisposable
    {
        private readonly IArAvailability _arAvailability;
        private readonly ICameraPermission _cameraPermission;
        private readonly IApplicationLifecycle _lifecycle;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _progressRepository;
        private readonly HuntLaunchOptions _options;
        private readonly ISceneLoader _sceneLoader;
        private readonly ObservableValue<LaunchPrompt> _prompt = new ObservableValue<LaunchPrompt>(LaunchPrompt.None);
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private bool _isBusy;

        public HuntLauncher(
            IArAvailability arAvailability,
            ICameraPermission cameraPermission,
            IApplicationLifecycle lifecycle,
            PlayerProgress progress,
            PlayerProgressRepository progressRepository,
            HuntLaunchOptions options,
            ISceneLoader sceneLoader)
        {
            _arAvailability = arAvailability;
            _cameraPermission = cameraPermission;
            _lifecycle = lifecycle;
            _progress = progress;
            _progressRepository = progressRepository;
            _options = options;
            _sceneLoader = sceneLoader;
        }

        public IReadOnlyObservableValue<LaunchPrompt> Prompt => _prompt;

        public void Initialize()
        {
            _lifecycle.Resumed += OnResumed;
        }

        public void Dispose()
        {
            _lifecycle.Resumed -= OnResumed;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public UniTask LaunchAsync(CancellationToken cancellationToken)
        {
            return RunExclusiveAsync(ResolveEnvironmentAsync, cancellationToken);
        }

        public UniTask AllowCameraAsync(CancellationToken cancellationToken)
        {
            switch (_prompt.Value)
            {
                case LaunchPrompt.CameraPermission:
                    return RunExclusiveAsync(RequestCameraAsync, cancellationToken);
                case LaunchPrompt.CameraSettings:
                    // The result arrives through OnResumed once the player comes back from the system settings.
                    _cameraPermission.OpenAppSettings();
                    return UniTask.CompletedTask;
                default:
                    return UniTask.CompletedTask;
            }
        }

        public UniTask PlayVirtualAsync(CancellationToken cancellationToken)
        {
            if (!IsCameraPrompt(_prompt.Value))
                return UniTask.CompletedTask;

            return RunExclusiveAsync(LoadVirtualAsync, cancellationToken);
        }

        public UniTask AcknowledgeVirtualRoomNoticeAsync(CancellationToken cancellationToken)
        {
            if (_prompt.Value != LaunchPrompt.VirtualRoomNotice)
                return UniTask.CompletedTask;

            _progress.MarkVirtualRoomNoticeShown();
            _progressRepository.Save(_progress);
            return RunExclusiveAsync(LoadVirtualAsync, cancellationToken);
        }

        private async UniTask RunExclusiveAsync(Func<CancellationToken, UniTask> step, CancellationToken cancellationToken)
        {
            // The permission dialog and ARCore install pause the app, so taps and Resumed can race an in-flight step.
            if (_isBusy)
                return;

            _isBusy = true;
            try
            {
                await step(cancellationToken);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async UniTask ResolveEnvironmentAsync(CancellationToken cancellationToken)
        {
            var availability = await _arAvailability.CheckAsync(cancellationToken);
            if (availability == ArAvailabilityResult.NeedsInstall)
            {
                var installed = await _arAvailability.TryInstallAsync(cancellationToken);
                availability = installed ? ArAvailabilityResult.Supported : ArAvailabilityResult.Unsupported;
            }

            if (availability != ArAvailabilityResult.Supported)
            {
                await ContinueWithoutArAsync(cancellationToken);
                return;
            }

            if (_cameraPermission.IsGranted)
            {
                await LoadAsync(HuntEnvironment.Ar, cancellationToken);
                return;
            }

            // The system dialog is only raised from the explanation screen's button: a player who knows why the
            // camera is needed rarely denies it, and Android stops showing the dialog after the second denial.
            _prompt.Value = LaunchPrompt.CameraPermission;
        }

        private async UniTask RequestCameraAsync(CancellationToken cancellationToken)
        {
            var result = await _cameraPermission.RequestAsync(cancellationToken);
            switch (result)
            {
                case PermissionResult.Granted:
                    await LoadAsync(HuntEnvironment.Ar, cancellationToken);
                    break;
                case PermissionResult.DeniedPermanently:
                    _prompt.Value = LaunchPrompt.CameraSettings;
                    break;
                default:
                    _prompt.Value = LaunchPrompt.CameraPermission;
                    break;
            }
        }

        private UniTask ContinueWithoutArAsync(CancellationToken cancellationToken)
        {
            if (_progress.VirtualRoomNoticeShown)
                return LoadVirtualAsync(cancellationToken);

            _prompt.Value = LaunchPrompt.VirtualRoomNotice;
            return UniTask.CompletedTask;
        }

        private UniTask LoadVirtualAsync(CancellationToken cancellationToken)
        {
            return LoadAsync(HuntEnvironment.Virtual, cancellationToken);
        }

        private UniTask LoadAsync(HuntEnvironment environment, CancellationToken cancellationToken)
        {
            _options.Select(environment);
            _prompt.Value = LaunchPrompt.None;
            return _sceneLoader.LoadAsync(SceneId.Hunt, cancellationToken);
        }

        private void OnResumed()
        {
            if (_isBusy || !IsCameraPrompt(_prompt.Value) || !_cameraPermission.IsGranted)
                return;

            RunExclusiveAsync(LoadArAsync, _lifetime.Token).Forget();
        }

        private UniTask LoadArAsync(CancellationToken cancellationToken)
        {
            return LoadAsync(HuntEnvironment.Ar, cancellationToken);
        }

        private static bool IsCameraPrompt(LaunchPrompt prompt)
        {
            return prompt == LaunchPrompt.CameraPermission || prompt == LaunchPrompt.CameraSettings;
        }
    }
}
