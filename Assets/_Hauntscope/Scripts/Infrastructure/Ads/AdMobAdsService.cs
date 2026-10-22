using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using UnityEngine;
using UnityEngine.Android;

namespace Hauntscope.Infrastructure.Ads
{
    // AdMob through a thin Java bridge (Plugins/Android/Ads.androidlib): Google's UMP consent first, then the SDK,
    // one rewarded and one interstitial kept loaded. The official Unity plugin would pull in the External
    // Dependency Manager for the same Java calls, like the Play libraries in PlayStore/.
    public sealed class AdMobAdsService : IAdsService, IAdPrivacy, IAdsInitReport, IDisposable
    {
        private const string BridgeClass = "com.pavko.hauntscope.ads.AdsBridge";
        private const string Earned = "earned";
        private const string ShowFailed = "show_failed";
        private const int MillisecondsPerSecond = 1000;
        private const string InitError = "error";

        private readonly AdUnitsConfig _units;
        private readonly AdsConfig _config;
        private readonly ICrashReporter _reporter;
        private readonly IApplicationLifecycle _lifecycle;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly UniTaskCompletionSource<bool> _initialized = new UniTaskCompletionSource<bool>();

        private AndroidJavaObject _bridge;
        private UniTaskCompletionSource<bool> _rewardedShow;
        private UniTaskCompletionSource<bool> _interstitialShow;
        private UniTaskCompletionSource _privacyShow;
        private int _attempt;
        private bool _isRetryScheduled;
        private bool _needsConsentRetry;

        public AdMobAdsService(AdUnitsConfig units, AdsConfig config, ICrashReporter reporter, IApplicationLifecycle lifecycle)
        {
            _units = units;
            _config = config;
            _reporter = reporter;
            _lifecycle = lifecycle;
            _lifecycle.Resumed += OnResumed;
        }

        public event Action<string, int> Reported;

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

        public UniTask<bool> ShowRewardedAsync(CancellationToken cancellationToken, string placement = "")
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
            _lifecycle.Resumed -= OnResumed;
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
                _attempt = 1;
                _bridge.Call("initialize");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: ads unavailable ({exception.Message}).");
                _reporter.RecordNonFatal(CrashKeys.AdsArea, exception);
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
                    OnInitialized(detail);
                    break;
                case "consent_error":
                    Debug.LogWarning($"Hauntscope: consent form unavailable ({detail}).");
                    _reporter.RecordNonFatal(CrashKeys.AdsArea, $"consent: {detail}");
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
                    ReportShowFailure(detail);
                    EndShow();
                    _rewardedShow?.TrySetResult(detail == Earned);
                    break;
                case "interstitial_closed":
                    ReportShowFailure(detail);
                    EndShow();
                    _interstitialShow?.TrySetResult(detail != ShowFailed);
                    break;
                case "privacy_closed":
                    _privacyShow?.TrySetResult();
                    break;
            }
        }

        // The first answer releases the boot (and the analytics consent); a failed request keeps being retried, so ads
        // start later in the same run instead of never (17 consent errors, 9 rewarded shows in 285 requests).
        private void OnInitialized(string result)
        {
            _initialized.TrySetResult(result == "ok");
            Reported?.Invoke(result, _attempt);
            _needsConsentRetry = result == InitError;
            if (_needsConsentRetry && _attempt <= _config.ConsentRetries)
                RetryConsentAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid RetryConsentAsync(CancellationToken cancellationToken)
        {
            if (_isRetryScheduled)
                return;

            _isRetryScheduled = true;
            var delay = _config.ConsentRetryDelay * (1 << Math.Min(_attempt - 1, _config.ConsentRetries));
            await UniTask.Delay((int)(delay * MillisecondsPerSecond), true, cancellationToken: cancellationToken);
            _isRetryScheduled = false;
            RetryConsent();
        }

        private void OnResumed()
        {
            if (_needsConsentRetry && !_isRetryScheduled && !IsShowing)
                RetryConsent();
        }

        private void RetryConsent()
        {
            if (_bridge == null || !_needsConsentRetry)
                return;

            _needsConsentRetry = false;
            _attempt++;
            _bridge.Call("retryConsent");
        }

        // A loaded ad that would not show is worth a look; a load without fill is everyday life and only a breadcrumb.
        private void ReportShowFailure(string detail)
        {
            if (detail == ShowFailed)
                _reporter.RecordNonFatal(CrashKeys.AdsArea, "loaded ad failed to show");
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
