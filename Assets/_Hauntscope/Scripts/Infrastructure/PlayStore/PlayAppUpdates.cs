using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using UnityEngine;
using UnityEngine.Android;

namespace Hauntscope.Infrastructure.PlayStore
{
    // Google Play In-App Updates, immediate flow: Play shows its full-screen update page over the game and restarts
    // the app when done. Sessions are short and balance ships with builds, so one tap to the latest version beats a
    // background download the player would have to finish later. Cancelling simply continues into the game.
    public sealed class PlayAppUpdates : IAppUpdates
    {
        private const int UpdateAvailable = 2;
        private const int DeveloperTriggeredUpdateInProgress = 3;
        private const int ImmediateUpdate = 1;
        private const int RequestCode = 5301;
        private const string StoreUrl = "market://details?id=";
        private const string WebStoreUrl = "https://play.google.com/store/apps/details?id=";

        private int? _installedVersionCode;

        public int InstalledVersionCode => _installedVersionCode ??= ReadVersionCode();

        public async UniTask<bool> TryStartUpdateAsync(CancellationToken cancellationToken)
        {
            try
            {
                var activity = AndroidApplication.currentActivity;
                using var factory = new AndroidJavaClass("com.google.android.play.core.appupdate.AppUpdateManagerFactory");
                using var manager = factory.CallStatic<AndroidJavaObject>("create", activity);
                using var request = manager.Call<AndroidJavaObject>("getAppUpdateInfo");
                using var info = await PlayTaskListener.AwaitAsync(request, cancellationToken);

                // An immediate update interrupted by closing the app is reported as in progress and must be resumed.
                var availability = info.Call<int>("updateAvailability");
                if (availability != UpdateAvailable && availability != DeveloperTriggeredUpdateInProgress)
                    return false;
                if (!info.Call<bool>("isUpdateTypeAllowed", ImmediateUpdate))
                    return false;

                using var optionsClass = new AndroidJavaClass("com.google.android.play.core.appupdate.AppUpdateOptions");
                using var options = optionsClass.CallStatic<AndroidJavaObject>("defaultOptions", ImmediateUpdate);
                return manager.Call<bool>("startUpdateFlowForResult", info, activity, options, RequestCode);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: update check unavailable ({exception.Message}).");
                return false;
            }
        }

        public void OpenStorePage()
        {
            try
            {
                Application.OpenURL(StoreUrl + Application.identifier);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: Play Store app unavailable ({exception.Message}).");
                Application.OpenURL(WebStoreUrl + Application.identifier);
            }
        }

        // Unreadable means "assume current": a wrong guess must never nag a player who is up to date.
        private static int ReadVersionCode()
        {
            try
            {
                var activity = AndroidApplication.currentActivity;
                using var packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
                using var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", Application.identifier, 0);
                return packageInfo.Get<int>("versionCode");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: installed version unreadable ({exception.Message}).");
                return int.MaxValue;
            }
        }
    }
}
