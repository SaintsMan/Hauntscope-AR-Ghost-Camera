#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Infrastructure.Firebase;
using UnityEngine;
using UnityEngine.Android;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Analytics
{
    // Adapter over Google Analytics for Firebase. What arrives before Firebase has started waits in a queue (a few
    // events on the first screens at most). Ad signals are never granted from analytics; analytics storage follows the
    // GDPR answer of the ads consent form, read again once that form has been dealt with on this launch.
    public sealed class FirebaseAnalyticsService : IAnalyticsService, IStartable, IDisposable
    {
        private const string GdprAppliesKey = "IABTCF_gdprApplies";
        private const string PurposeConsentsKey = "IABTCF_PurposeConsents";
        private const int GdprUnknown = -1;

        private readonly FirebaseGate _gate;
        private readonly IAdsService _ads;
        private readonly List<Action> _pending = new List<Action>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool? _available;

        public FirebaseAnalyticsService(FirebaseGate gate, IAdsService ads)
        {
            _gate = gate;
            _ads = ads;
        }

        public void Start()
        {
            StartAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public void Log(string eventName)
        {
            Send(() => FirebaseAnalytics.LogEvent(eventName));
        }

        public void Log(string eventName, params AnalyticsParameter[] parameters)
        {
            var converted = new Parameter[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                converted[i] = parameter.IsNumber ? new Parameter(parameter.Name, parameter.Number) : new Parameter(parameter.Name, parameter.Text);
            }

            Send(() => FirebaseAnalytics.LogEvent(eventName, converted));
        }

        public void SetUserProperty(string name, string value)
        {
            Send(() => FirebaseAnalytics.SetUserProperty(name, value));
        }

        private void Send(Action call)
        {
            if (_available == true)
                call();
            else if (_available == null)
                _pending.Add(call);
        }

        private async UniTaskVoid StartAsync(CancellationToken cancellationToken)
        {
            _available = await _gate.StartAsync(cancellationToken);
            if (_available == true)
            {
                ApplyConsent();
                foreach (var call in _pending)
                    call();
            }

            _pending.Clear();
            if (_available != true)
                return;

            await _ads.InitializeAsync(cancellationToken);
            ApplyConsent();
        }

        private static void ApplyConsent()
        {
            var analytics = ReadAnalyticsConsent() ? ConsentStatus.Granted : ConsentStatus.Denied;
            FirebaseAnalytics.SetConsent(new Dictionary<ConsentType, ConsentStatus>
            {
                { ConsentType.AnalyticsStorage, analytics },
                { ConsentType.AdStorage, ConsentStatus.Denied },
                { ConsentType.AdUserData, ConsentStatus.Denied },
                { ConsentType.AdPersonalization, ConsentStatus.Denied }
            });
        }

        private static bool ReadAnalyticsConsent()
        {
            try
            {
                using var preferenceManager = new AndroidJavaClass("android.preference.PreferenceManager");
                using var preferences = preferenceManager.CallStatic<AndroidJavaObject>("getDefaultSharedPreferences", AndroidApplication.currentActivity);
                var gdprApplies = preferences.Call<int>("getInt", GdprAppliesKey, GdprUnknown);
                var purposes = preferences.Call<string>("getString", PurposeConsentsKey, string.Empty);
                return AnalyticsConsentPolicy.AllowsAnalytics(gdprApplies, purposes);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: consent not read ({exception.Message}); analytics storage stays on.");
                return true;
            }
        }
    }
}
#endif
