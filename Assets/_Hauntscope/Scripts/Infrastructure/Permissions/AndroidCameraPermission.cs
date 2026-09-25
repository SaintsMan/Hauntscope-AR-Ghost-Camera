using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Environment;
using UnityEngine;
using UnityEngine.Android;

namespace Hauntscope.Infrastructure.Permissions
{
    public sealed class AndroidCameraPermission : ICameraPermission
    {
        private const string AppDetailsSettingsAction = "android.settings.APPLICATION_DETAILS_SETTINGS";
        private const string PackageUriPrefix = "package:";
        private const int FlagActivityNewTask = 0x10000000;

        public bool IsGranted => Permission.HasUserAuthorizedPermission(Permission.Camera);

        public async UniTask<PermissionResult> RequestAsync(CancellationToken cancellationToken)
        {
            if (IsGranted)
                return PermissionResult.Granted;

            var answer = new UniTaskCompletionSource<Answer>();
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => answer.TrySetResult(Answer.Granted);
            callbacks.PermissionDenied += _ => answer.TrySetResult(Answer.Denied);
            callbacks.PermissionRequestDismissed += _ => answer.TrySetResult(Answer.Dismissed);
            Permission.RequestUserPermission(Permission.Camera, callbacks);

            var result = await answer.Task.AttachExternalCancellation(cancellationToken);
            // Permission callbacks arrive on the Java UI thread; the JNI query and the caller's continuation need the main thread.
            await UniTask.SwitchToMainThread(cancellationToken);
            return ToResult(result);
        }

        public void OpenAppSettings()
        {
            using var uriClass = new AndroidJavaClass("android.net.Uri");
            using var uri = uriClass.CallStatic<AndroidJavaObject>("parse", PackageUriPrefix + Application.identifier);
            using var intent = new AndroidJavaObject("android.content.Intent", AppDetailsSettingsAction, uri);
            intent.Call<AndroidJavaObject>("addFlags", FlagActivityNewTask).Dispose();
            AndroidApplication.currentActivity.Call("startActivity", intent);
        }

        private static PermissionResult ToResult(Answer answer)
        {
            switch (answer)
            {
                case Answer.Granted:
                    return PermissionResult.Granted;
                case Answer.Denied:
                    // After a denial Android stops offering the rationale only when "Don't ask again" is in effect
                    // (explicitly, or automatically after the second denial on Android 11+).
                    return Permission.ShouldShowRequestPermissionRationale(Permission.Camera)
                        ? PermissionResult.Denied
                        : PermissionResult.DeniedPermanently;
                default:
                    // Dismissing the dialog (tapping outside it) is not a decision, so the system dialog stays available.
                    return PermissionResult.Denied;
            }
        }

        private enum Answer
        {
            Granted,
            Denied,
            Dismissed
        }
    }
}
