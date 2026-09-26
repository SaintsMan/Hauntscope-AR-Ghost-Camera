using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using UnityEngine;
using UnityEngine.Android;

namespace Hauntscope.Infrastructure.Ads
{
    // AdMob through a thin Java bridge (Plugins/Android/Ads.androidlib): Google's UMP consent first, then the SDK,
    // one rewarded and one interstitial kept loaded. The official Unity plugin would pull in the External
    // Dependency Manager for the same Java calls, like the Play libraries in PlayStore/.
    public sealed class AdMobAdsService : IAdsService, IAdPrivacy, IDisposable
    {
        private const string BridgeClass = "com.pavko.hauntscope.ads.AdsBridge";
        private const string Earned = "earned";
        private const int MillisecondsPerSecond = 1000;

        private readonly AdUnitsConfig _units;
        private readonly AdsConfig _config;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly UniTaskCompletionSource<bool> _initialized = new UniTaskCompletionSource<bool>();

        private AndroidJavaObject _bridge;
        private UniTaskCompletionSource<bool> _rewardedShow;
        private UniTaskCompletionSource<bool> _interstitialShow;
        private UniTaskCompletionSource _privacyShow;

        public AdMobAdsService(AdUnitsConfig units, AdsConfig config)
        {
            _units = units;
            _config = config;
        }

        public bool IsRewardedReady { get; private set; }

        public bool IsInterstitialReady { get; private set; }

        public bool IsShowing { get; private set; }

        public bool IsOptionsRequired => _bridge != null && _bridge.Call<bool>("isPrivacyOptionsRequired");

        public event Action AvailabilityChanged;

        // One initialisation per app run; the boot step only waits for it, so a timed-out step never cancels it.
        public async UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            if (_bridge == null)
                StartBridge();

            return await _initialized.Task.AttachExternalCancellation(cancellationToken);
        }

        public UniTask<bool> ShowRewardedAsync(CancellationToken cancellationToken)
        {
            if (!IsRewardedReady || IsShowing)
                return UniTask.FromResult(false);

            _rewardedShow = new UniTaskCompletionSource<bool>();
            BeginShow();
            IsRewardedReady = false;
            _bridge.Call("showRewarded");
            return _rewardedShow.Task.AttachExternalCancellation(cancellationToken);
        }

        public UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken)
        {
            if (!IsInterstitialReady || IsShowing)
                return UniTask.FromResult(false);

            _interstitialShow = new UniTaskCompletionSource<bool>();
            BeginShow();
            IsInterstitialReady = false;
            _bridge.Call("showInterstitial");
            return _interstitialShow.Task.AttachExternalCancellation(cancellationToken);
        }

        public UniTask ShowOptionsAsync(CancellationToken cancellationToken)
        {
            if (_bridge == null)
                return UniTask.CompletedTask;

            _privacyShow = new UniTaskCompletionSource();
            _bridge.Call("showPrivacyOptions");
            return _privacyShow.Task.AttachExternalCancellation(cancellationToken);
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            _bridge?.Dispose();
        }

        private void StartBridge()
        {
            try
            {
                _bridge = new AndroidJavaObject(BridgeClass, AndroidApplication.currentActivity, _units.RewardedUnitId,
                    _units.InterstitialUnitId, new AdsBridgeListener(OnEvent));
                _bridge.Call("initialize");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: ads unavailable ({exception.Message}).");
                _initialized.TrySetResult(false);
            }
        }

        // The game is paused by the OS while an ad is up; flagging it first lets HuntPause ignore that pause.
        private void BeginShow()
        {
            IsShowing = true;
            AvailabilityChanged?.Invoke();
        }

        private void EndShow()
        {
            IsShowing = false;
            AvailabilityChanged?.Invoke();
        }

        private void OnEvent(string name, string detail)
        {
            switch (name)
            {
                case "init":
                    _initialized.TrySetResult(detail == "ok");
                    break;
                case "consent_error":
                    Debug.LogWarning($"Hauntscope: consent form unavailable ({detail}).");
                    break;
                case "rewarded_loaded":
                    IsRewardedReady = true;
                    AvailabilityChanged?.Invoke();
                    break;
                case "interstitial_loaded":
                    IsInterstitialReady = true;
                    AvailabilityChanged?.Invoke();
                    break;
                case "rewarded_failed":
                    RetryLater("loadRewarded", detail);
                    break;
                case "interstitial_failed":
                    RetryLater("loadInterstitial", detail);
                    break;
                case "rewarded_closed":
                    EndShow();
                    _rewardedShow?.TrySetResult(detail == Earned);
                    break;
                case "interstitial_closed":
                    EndShow();
                    _interstitialShow?.TrySetResult(detail != "show_failed");
                    break;
                case "privacy_closed":
                    _privacyShow?.TrySetResult();
                    break;
            }
        }

        // No fill or no network: try again after a pause instead of hammering the ad server.
        private void RetryLater(string loadMethod, string reason)
        {
            Debug.LogWarning($"Hauntscope: ad failed to load ({reason}), retrying.");
            RetryAsync(loadMethod, _lifetime.Token).Forget();
        }

        private async UniTaskVoid RetryAsync(string loadMethod, CancellationToken cancellationToken)
        {
            await UniTask.Delay((int)(_config.ReloadDelay * MillisecondsPerSecond), true, cancellationToken: cancellationToken);
            _bridge?.Call(loadMethod);
        }
    }
}
